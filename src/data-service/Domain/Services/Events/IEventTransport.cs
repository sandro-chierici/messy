namespace DataService.Domain.Services.Events;

/// <summary>
/// An event as stored in the outbox, ready to be handed to the broker.
/// </summary>
/// <param name="Id">Outbox row id</param>
/// <param name="EventId">Id of the event, used by the broker to drop duplicates</param>
/// <param name="Subject">Broker subject, e.g. events.tenant.createdevent</param>
/// <param name="EventType">Name of the event class</param>
/// <param name="Payload">JSON of the event</param>
public record OutboxMessage(long Id, Guid EventId, string Subject, string EventType, string Payload);

/// <summary>
/// Delivers outbox messages to the broker. Throws when the broker did not accept the message.
/// </summary>
public interface IEventTransport
{
    Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken);
}
