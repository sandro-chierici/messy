namespace DataService.Domain.Rules;

/// <summary>
/// Configuration of the event pipeline: outbox, relay and broker
/// </summary>
public class EventsOptions
{
    public const string Nats = "Nats";
    public const string InMemory = "InMemory";

    /// <summary>
    /// Broker used by the outbox relay: Nats or InMemory
    /// </summary>
    public string Transport { get; set; } = Nats;

    public OutboxOptions Outbox { get; set; } = new();
    public NatsOptions NatsJetStream { get; set; } = new();

    public class OutboxOptions
    {
        /// <summary>Max rows relayed per batch</summary>
        public int BatchSize { get; set; } = 100;
        /// <summary>Wait when the outbox is empty or the broker is failing</summary>
        public TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(500);
        /// <summary>After this many failed deliveries a row is parked (dead_utc_date) and skipped</summary>
        public int MaxAttempts { get; set; } = 10;
        /// <summary>Relayed rows older than this are deleted</summary>
        public TimeSpan SentRetention { get; set; } = TimeSpan.FromDays(3);
    }

    public class NatsOptions
    {
        public string Url { get; set; } = "nats://localhost:4222";
        public string StreamName { get; set; } = "EVENTS";
        public string[] Subjects { get; set; } = ["events.>"];
        /// <summary>WorkQueue (message removed once acked, competing consumers) or Limits</summary>
        public string Retention { get; set; } = "WorkQueue";
        public TimeSpan MaxAge { get; set; } = TimeSpan.FromDays(7);
        /// <summary>Window in which the broker drops a message with an already seen event id</summary>
        public TimeSpan DuplicateWindow { get; set; } = TimeSpan.FromMinutes(10);
        public int Replicas { get; set; } = 1;
    }
}
