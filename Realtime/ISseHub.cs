using System.Threading.Channels;

namespace QuizForge.Realtime;

public interface ISseHub
{
    SseSubscription Subscribe(string streamKey, long? lastEventId = null);
    ValueTask<int> PublishToStreamAsync(
        string streamKey,
        string eventName,
        object? data,
        CancellationToken cancellationToken = default
    );
    ValueTask<int> PublishToUserAsync(
        long userId,
        string eventName,
        object? data,
        CancellationToken cancellationToken = default
    );
}

public sealed class SseSubscription(
    string connectionId,
    ChannelReader<SseEventEnvelope> reader,
    Func<ValueTask> onDispose
) : IAsyncDisposable
{
    private int _disposed;
    private readonly Func<ValueTask> _onDispose = onDispose;

    public string ConnectionId { get; } = connectionId;
    public ChannelReader<SseEventEnvelope> Reader { get; } = reader;

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        await _onDispose();
    }
}

public sealed record SseEventEnvelope(
    long Id,
    string EventName,
    string Data,
    DateTimeOffset CreatedAt
);
