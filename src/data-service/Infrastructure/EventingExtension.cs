using DataService.Domain.Rules;
using DataService.Domain.Services.Events;
using DataService.Infrastructure.Services.Events;

namespace DataService.Infrastructure;

public static class EventingExtension
{
    /// <summary>
    /// Registers the event pipeline: services and repositories write events to the outbox
    /// (in the business transaction, or on their own for failures), OutboxRelayService relays them to the broker
    /// chosen by Events:Transport (Nats | InMemory).
    /// </summary>
    public static IServiceCollection AddEventing(this IServiceCollection @this, IConfiguration configuration)
    {
        @this.Configure<EventsOptions>(configuration.GetSection("Events"));

        @this.AddSingleton<IOutboxWriter, PostgresOutboxWriter>();
        @this.AddScoped<IEventPublisher, OutboxEventPublisher>();

        var transport = configuration["Events:Transport"] ?? EventsOptions.Nats;
        if (transport.Equals(EventsOptions.InMemory, StringComparison.OrdinalIgnoreCase))
        {
            @this.AddSingleton<InMemoryEventTransport>();
            @this.AddSingleton<IEventTransport>(sp => sp.GetRequiredService<InMemoryEventTransport>());
        }
        else if (transport.Equals(EventsOptions.Nats, StringComparison.OrdinalIgnoreCase))
        {
            @this.AddSingleton<NatsEventTransport>();
            @this.AddSingleton<IEventTransport>(sp => sp.GetRequiredService<NatsEventTransport>());
        }
        else
        {
            throw new InvalidOperationException($"Events:Transport '{transport}' is not supported. Use '{EventsOptions.Nats}' or '{EventsOptions.InMemory}'.");
        }

        @this.AddHostedService<OutboxRelayService>();

        return @this;
    }
}
