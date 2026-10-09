using DataService.Domain.IO;
using DataService.Domain.Services.Events;

namespace DataService.Infrastructure.Services.Events;

/// <summary>
/// Scoped publisher for events that are not tied to a database transaction (e.g. FailureEvent):
/// the event is stored in the outbox with its own connection and relayed later.
/// Best effort: when the outbox itself is unavailable the error is logged, so it never hides the original failure.
/// Events that must be atomic with a business change go through IOutboxWriter inside the repository transaction.
/// </summary>
public class OutboxEventPublisher(
    IDbConnectionFactory dbConnectionFactory,
    IOutboxWriter outboxWriter,
    ILogger<OutboxEventPublisher> logger) : IEventPublisher
{
    public async Task PublishEventAsync(EventBase @event)
    {
        try
        {
            using var conn = await dbConnectionFactory.CreateConnectionAsync();
            using var transaction = conn.BeginTransaction();
            await outboxWriter.AddAsync(conn, transaction, @event);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not store event {EventType} {EventId} in the outbox", @event.EventType, @event.Id);
        }
    }
}
