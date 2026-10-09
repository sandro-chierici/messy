using DataService.Domain.Rules;
using DataService.Domain.Services.Events;
using Microsoft.Extensions.Options;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using NATS.Net;
using System.Text;

namespace DataService.Infrastructure.Services.Events;

/// <summary>
/// Singleton transport to NATS JetStream. The stream is created on first use, so the application
/// starts even when the broker is down. The event id is sent as Nats-Msg-Id: the broker drops a message
/// relayed twice within the duplicate window, which makes relay retries safe.
/// </summary>
public sealed class NatsEventTransport : IEventTransport, IAsyncDisposable
{
    private readonly EventsOptions.NatsOptions _options;
    private readonly NatsClient _client;
    private readonly INatsJSContext _jetStream;
    private readonly SemaphoreSlim _streamLock = new(1, 1);
    private volatile bool _streamReady;

    public NatsEventTransport(IOptions<EventsOptions> options)
    {
        _options = options.Value.NatsJetStream;
        _client = new NatsClient(_options.Url);
        _jetStream = _client.CreateJetStreamContext();
    }

    public async Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        await EnsureStreamAsync(cancellationToken);

        var ack = await _jetStream.PublishAsync(
            message.Subject,
            Encoding.UTF8.GetBytes(message.Payload),
            opts: new NatsJSPubOpts { MsgId = message.EventId.ToString() },
            headers: new NatsHeaders { { "event-type", message.EventType } },
            cancellationToken: cancellationToken);

        ack.EnsureSuccess();
    }

    private async Task EnsureStreamAsync(CancellationToken cancellationToken)
    {
        if (_streamReady)
            return;

        await _streamLock.WaitAsync(cancellationToken);
        try
        {
            if (_streamReady)
                return;

            await _jetStream.CreateOrUpdateStreamAsync(
                new StreamConfig(_options.StreamName, _options.Subjects)
                {
                    Retention = Enum.Parse<StreamConfigRetention>(_options.Retention, ignoreCase: true),
                    MaxAge = _options.MaxAge,
                    DuplicateWindow = _options.DuplicateWindow,
                    NumReplicas = _options.Replicas,
                    Storage = StreamConfigStorage.File
                },
                cancellationToken);

            _streamReady = true;
        }
        finally
        {
            _streamLock.Release();
        }
    }

    public async ValueTask DisposeAsync() => await _client.DisposeAsync();
}
