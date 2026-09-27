namespace DataService.Domain.Services.Events;

public interface IEventPublisher
{
    Task PublishEventAsync(EventBase @event);
}
