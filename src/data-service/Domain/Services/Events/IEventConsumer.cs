namespace DataService.Domain.Services.Events
{
    public interface IEventConsumer
    {
        ValueTask<EventBase> ConsumeEventAsync(EventBase @event);
    }
}
