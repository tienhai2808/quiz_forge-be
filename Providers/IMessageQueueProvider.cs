using QuizForge.DTOs;

namespace QuizForge.Providers;

public interface IMessageQueueProvider
{
    Task PublishAsync(ExtractDocumentMessageDto message, CancellationToken cancellationToken = default);
    Task ConsumeAsync(Func<ExtractDocumentMessageDto, CancellationToken, Task> messageHandler, CancellationToken cancellationToken = default);
}
