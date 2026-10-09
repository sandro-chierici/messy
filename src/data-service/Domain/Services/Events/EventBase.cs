namespace DataService.Domain.Services.Events;

public abstract class EventBase
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Producer { get; set; }
    public string EventType => GetType().Name;
    public string? ResourceName { get; set; }
    public string? ResourceUri { get; set; } 
    public string? ResourceId { get; set; }
    public string? ResourceType { get; set; }
}

public class CreatedEvent : EventBase
{
}

public class FailureEvent : EventBase
{
    public string? ErrorMessage { get; set; }
}