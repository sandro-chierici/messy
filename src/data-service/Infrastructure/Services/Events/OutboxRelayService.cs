using Dapper;
using DataService.Domain.IO;
using DataService.Domain.Rules;
using DataService.Domain.Services.Events;
using Microsoft.Extensions.Options;

namespace DataService.Infrastructure.Services.Events;

/// <summary>
/// Background service that relays the rows of event_outbox to the broker (IEventTransport), oldest first.
/// Delivery is at-least-once: a row is marked as sent only after the broker accepted it, and the broker
/// drops duplicates by event id. Several instances can run together (FOR UPDATE SKIP LOCKED), but then
/// the order between events is no longer guaranteed.
/// A failing row is retried with a growing delay; after MaxAttempts it is parked (dead_utc_date) so it does not block the others.
/// </summary>
public class OutboxRelayService(
    IServiceScopeFactory scopeFactory,
    IEventTransport transport,
    IOptions<EventsOptions> options,
    ILogger<OutboxRelayService> logger) : BackgroundService
{
    private static readonly TimeSpan PurgeEvery = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan MaxBackoff = TimeSpan.FromSeconds(30);
    private const int MaxErrorLength = 1000;

    private sealed class OutboxRow
    {
        public long Id { get; set; }
        public Guid EventId { get; set; }
        public string Subject { get; set; } = "";
        public string EventType { get; set; } = "";
        public string Payload { get; set; } = "";
        public int Attempts { get; set; }
    }

    private readonly EventsOptions.OutboxOptions _options = options.Value.Outbox;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consecutiveFailures = 0;
        var lastPurge = DateTime.UtcNow;

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeSpan.Zero;
            try
            {
                var result = await RelayBatchAsync(stoppingToken);

                if (result.Failed)
                {
                    consecutiveFailures++;
                    delay = Backoff(consecutiveFailures);
                }
                else
                {
                    consecutiveFailures = 0;
                    // a full batch means there is probably more to send: do not wait
                    if (result.Sent < _options.BatchSize)
                        delay = _options.PollInterval;
                }

                if (DateTime.UtcNow - lastPurge >= PurgeEvery)
                {
                    lastPurge = DateTime.UtcNow;
                    await PurgeAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // typically the database is not reachable
                consecutiveFailures++;
                delay = Backoff(consecutiveFailures);
                logger.LogError(ex, "Outbox relay iteration failed, retrying in {Delay}", delay);
            }

            if (delay > TimeSpan.Zero)
            {
                try { await Task.Delay(delay, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }
    }

    private TimeSpan Backoff(int consecutiveFailures)
    {
        var factor = Math.Pow(2, Math.Min(consecutiveFailures, 10));
        var delay = TimeSpan.FromTicks((long)(_options.PollInterval.Ticks * factor));
        return delay > MaxBackoff ? MaxBackoff : delay;
    }

    private async Task<(int Sent, bool Failed)> RelayBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbConnectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();

        using var conn = await dbConnectionFactory.CreateConnectionAsync();
        using var transaction = conn.BeginTransaction();

        var rows = (await conn.QueryAsync<OutboxRow>(
            @"SELECT id, event_id, subject, event_type, payload::text AS payload, attempts
              FROM event_outbox
              WHERE sent_utc_date IS NULL AND dead_utc_date IS NULL
              ORDER BY id
              LIMIT @BatchSize
              FOR UPDATE SKIP LOCKED",
            new { _options.BatchSize },
            transaction)).ToList();

        if (rows.Count == 0)
            return (0, false);

        var sentIds = new List<long>(rows.Count);
        OutboxRow? failedRow = null;
        string? failure = null;

        foreach (var row in rows)
        {
            try
            {
                await transport.PublishAsync(
                    new OutboxMessage(row.Id, row.EventId, row.Subject, row.EventType, row.Payload),
                    cancellationToken);
                sentIds.Add(row.Id);
            }
            catch (Exception ex) when (cancellationToken.IsCancellationRequested)
            {
                // shutting down: keep what was already sent, the rest stays pending
                logger.LogDebug(ex, "Outbox relay stopped while publishing event {EventId}", row.EventId);
                break;
            }
            catch (Exception ex)
            {
                failedRow = row;
                failure = ex.Message.Length > MaxErrorLength ? ex.Message[..MaxErrorLength] : ex.Message;
                logger.LogWarning(ex, "Could not publish event {EventId} ({EventType}), attempt {Attempt}",
                    row.EventId, row.EventType, row.Attempts + 1);
                break;
            }
        }

        var now = DateTime.UtcNow;

        if (sentIds.Count > 0)
        {
            await conn.ExecuteAsync(
                "UPDATE event_outbox SET sent_utc_date = @Now WHERE id = ANY(@Ids)",
                new { Now = now, Ids = sentIds.ToArray() },
                transaction);
        }

        if (failedRow != null)
        {
            var parked = failedRow.Attempts + 1 >= _options.MaxAttempts;
            await conn.ExecuteAsync(
                @"UPDATE event_outbox
                  SET attempts = attempts + 1, last_error = @Error, dead_utc_date = @DeadDate
                  WHERE id = @Id",
                new { failedRow.Id, Error = failure, DeadDate = parked ? now : (DateTime?)null },
                transaction);

            if (parked)
                logger.LogError("Event {EventId} ({EventType}) parked in the outbox after {Attempts} failed attempts: {Error}",
                    failedRow.EventId, failedRow.EventType, failedRow.Attempts + 1, failure);
        }

        // the commit must not be skipped on shutdown, otherwise sent rows are relayed again
        transaction.Commit();

        return (sentIds.Count, failedRow != null);
    }

    private async Task PurgeAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbConnectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        using var conn = await dbConnectionFactory.CreateConnectionAsync();

        var purged = await conn.ExecuteAsync(
            "DELETE FROM event_outbox WHERE sent_utc_date < @Cutoff",
            new { Cutoff = DateTime.UtcNow - _options.SentRetention });

        if (purged > 0)
            logger.LogInformation("Purged {Count} relayed events from the outbox", purged);
    }
}
