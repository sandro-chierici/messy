using DataService.Domain.Services.Events;
using System.Collections.Concurrent;

namespace DataService.Infrastructure.Services.Events;

/// <summary>
/// Transport that keeps the relayed messages in memory instead of sending them to a broker (Events:Transport = InMemory).
/// </summary>
public class InMemoryEventTransport : IEventTransport
{
    private readonly ConcurrentQueue<OutboxMessage> _messages = new();

    public IReadOnlyCollection<OutboxMessage> Messages => _messages;

    public Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        _messages.Enqueue(message);
        return Task.CompletedTask;
    }
}
