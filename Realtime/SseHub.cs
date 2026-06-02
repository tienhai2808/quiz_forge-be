using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Options;
using QuizForge.Options;

namespace QuizForge.Realtime;

public class SseHub(IOptions<SseOptions> sseOptions, ILogger<SseHub> logger) : ISseHub
{
    private readonly ILogger<SseHub> _logger = logger;
    private readonly int _clientQueueCapacity = sseOptions.Value.ClientQueueCapacity;
    private readonly int _replayCapacityPerStream = sseOptions.Value.ReplayCapacityPerStream;
    private readonly ConcurrentDictionary<string, StreamState> _streams = new(StringComparer.Ordinal);
    private long _eventSequence;

    public SseSubscription Subscribe(string streamKey, long? lastEventId = null)
    {
        if (string.IsNullOrWhiteSpace(streamKey))
            throw new ArgumentException("Stream key is required.", nameof(streamKey));

        var state = _streams.GetOrAdd(streamKey, static _ => new StreamState());
        var channel = Channel.CreateBounded<SseEventEnvelope>(new BoundedChannelOptions(_clientQueueCapacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });

        var connectionId = $"{streamKey}:{Guid.NewGuid():N}";
        var connection = new SseConnection(connectionId, channel);
        if (!state.Clients.TryAdd(connectionId, connection))
            throw new InvalidOperationException("Failed to create SSE subscription.");

        if (lastEventId is not null)
        {
            foreach (var replayEvent in state.GetReplayAfter(lastEventId.Value))
            {
                if (!connection.Writer.TryWrite(replayEvent))
                {
                    _logger.LogWarning(
                        "Failed to enqueue replay event for SSE connection {ConnectionId}.",
                        connectionId
                    );
                    break;
                }
            }
        }

        _logger.LogInformation(
            "SSE client connected. Stream={StreamKey}, ConnectionId={ConnectionId}, OnlineClients={OnlineClients}",
            streamKey,
            connectionId,
            state.Clients.Count
        );

        return new SseSubscription(
            connectionId,
            connection.Reader,
            () =>
            {
                RemoveConnection(streamKey, connectionId);
                return ValueTask.CompletedTask;
            }
        );
    }

    public ValueTask<int> PublishToUserAsync(
        long userId,
        string eventName,
        object? data,
        CancellationToken cancellationToken = default
    ) => PublishToStreamAsync(GetUserStreamKey(userId), eventName, data, cancellationToken);

    public ValueTask<int> PublishToStreamAsync(
        string streamKey,
        string eventName,
        object? data,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_streams.TryGetValue(streamKey, out var state) || state.Clients.IsEmpty)
            return ValueTask.FromResult(0);

        var payload = JsonSerializer.Serialize(data);
        var envelope = new SseEventEnvelope(
            Interlocked.Increment(ref _eventSequence),
            eventName,
            payload,
            DateTimeOffset.UtcNow
        );

        state.AppendReplay(envelope, _replayCapacityPerStream);

        var delivered = 0;
        foreach (var connection in state.Clients.Values)
        {
            if (connection.Writer.TryWrite(envelope))
            {
                delivered++;
                continue;
            }

            _logger.LogWarning(
                "SSE enqueue failed because the channel is closed. ConnectionId={ConnectionId}",
                connection.ConnectionId
            );
            RemoveConnection(streamKey, connection.ConnectionId);
        }

        return ValueTask.FromResult(delivered);
    }

    public static string GetUserStreamKey(long userId) => $"user:{userId}";

    private void RemoveConnection(string streamKey, string connectionId)
    {
        if (!_streams.TryGetValue(streamKey, out var state))
            return;

        if (!state.Clients.TryRemove(connectionId, out var connection))
            return;

        connection.Complete();
        _logger.LogInformation(
            "SSE client disconnected. Stream={StreamKey}, ConnectionId={ConnectionId}, OnlineClients={OnlineClients}",
            streamKey,
            connectionId,
            state.Clients.Count
        );

        if (state.Clients.IsEmpty)
            _streams.TryRemove(new KeyValuePair<string, StreamState>(streamKey, state));
    }

    private sealed class SseConnection(string connectionId, Channel<SseEventEnvelope> channel)
    {
        private readonly Channel<SseEventEnvelope> _channel = channel;
        private int _completed;

        public string ConnectionId { get; } = connectionId;
        public ChannelWriter<SseEventEnvelope> Writer => _channel.Writer;
        public ChannelReader<SseEventEnvelope> Reader => _channel.Reader;

        public void Complete()
        {
            if (Interlocked.Exchange(ref _completed, 1) == 1)
                return;

            _channel.Writer.TryComplete();
        }
    }

    private sealed class StreamState
    {
        private readonly Queue<SseEventEnvelope> _replayBuffer = new();
        private readonly Lock _replayLock = new();

        public ConcurrentDictionary<string, SseConnection> Clients { get; } = new(StringComparer.Ordinal);

        public void AppendReplay(SseEventEnvelope envelope, int replayCapacity)
        {
            lock (_replayLock)
            {
                _replayBuffer.Enqueue(envelope);
                while (_replayBuffer.Count > replayCapacity)
                    _replayBuffer.Dequeue();
            }
        }

        public SseEventEnvelope[] GetReplayAfter(long lastEventId)
        {
            lock (_replayLock)
                return _replayBuffer.Where(x => x.Id > lastEventId).ToArray();
        }
    }
}
