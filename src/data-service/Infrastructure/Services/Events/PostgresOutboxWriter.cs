using Dapper;
using DataService.Domain.Services.Events;
using System.Data;

namespace DataService.Infrastructure.Services.Events;

/// <summary>
/// Singleton, stateless writer of the event_outbox table
/// </summary>
public class PostgresOutboxWriter : IOutboxWriter
{
    public async Task AddAsync(IDbConnection connection, IDbTransaction transaction, EventBase @event)
    {
        await connection.ExecuteAsync(
            @"INSERT INTO event_outbox (event_id, subject, event_type, payload, created_utc_date)
              VALUES (@EventId, @Subject, @EventType, @Payload::jsonb, @CreatedUTCDate)",
            new
            {
                EventId = @event.Id,
                Subject = EventSerializer.Subject(@event),
                @event.EventType,
                Payload = EventSerializer.Serialize(@event),
                CreatedUTCDate = DateTime.UtcNow
            },
            transaction);
    }
}
