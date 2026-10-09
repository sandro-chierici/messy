using DataService.Domain.Services.Events;
using System.Text.Json;

namespace DataService.Infrastructure.Services.Events;

/// <summary>
/// Turns an event into the JSON payload and the broker subject stored in the outbox
/// </summary>
public static class EventSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Serializes with the runtime type, so derived properties (e.g. ErrorMessage) are included
    /// </summary>
    public static string Serialize(EventBase @event)
        => JsonSerializer.Serialize(@event, @event.GetType(), JsonOptions);

    /// <summary>
    /// events.{resourcetype}.{eventtype}, e.g. events.tenant.createdevent. Characters not allowed in a NATS token are replaced.
    /// </summary>
    public static string Subject(EventBase @event)
        => $"events.{Token(@event.ResourceType)}.{Token(@event.EventType)}";

    private static string Token(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "unknown";
        return new string(value.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '_')
            .ToArray());
    }
}
