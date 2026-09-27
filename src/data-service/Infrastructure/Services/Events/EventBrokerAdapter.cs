using DataService.Domain.Services.Events;
using System.Threading.Channels;

namespace DataService.Infrastructure.Services.Events
{
    /// <summary>
    /// Singleton service that adapts the event broker to the domain event publisher and consumer interfaces
    /// </summary>
    public class EventBrokerAdapter : 
        IEventPublisher,
        IEventConsumer
    {
        private readonly Channel<EventBase> _eventChannel;

        public EventBrokerAdapter()
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
        public async ValueTask<EventBase> ConsumeEventAsync(EventBase @event)
        {   
            return  await _eventChannel.Reader.ReadAsync();
        }
    }
}
