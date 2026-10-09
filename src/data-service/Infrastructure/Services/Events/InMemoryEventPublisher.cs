using DataService.Domain.Services.Events;
using System.Threading.Channels;

namespace DataService.Infrastructure.Services.Events
{
    /// <summary>
    /// In-memory publisher and consumer, backed by a channel. Meant for tests, without database or broker.
    /// Register one singleton instance for both IEventPublisher and IEventConsumer.
    /// </summary>
    public class InMemoryEventPublisher :
        IEventPublisher,
        IEventConsumer
    {
        private readonly Channel<EventBase> _eventChannel;

        public InMemoryEventPublisher()
        {
            _eventChannel = Channel.CreateUnbounded<EventBase>(new UnboundedChannelOptions
            {
                SingleWriter = false,
                SingleReader = false
            });
        }

        public async Task PublishEventAsync(EventBase @event)
        {
            await _eventChannel.Writer.WriteAsync(@event);
        }
        public async ValueTask<EventBase> ConsumeEventAsync()
        {
            return  await _eventChannel.Reader.ReadAsync();
        }
    }
}
