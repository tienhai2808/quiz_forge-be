using QuizForge.Models;

namespace QuizForge.Repositories;

public interface IDocumentRepository
{
    Task CreateAsync(Document document, CancellationToken cancellationToken = default);

    Task<Document?> FindByFileHashSha256Async(string fileHashSha256, CancellationToken cancellationToken = default);
}