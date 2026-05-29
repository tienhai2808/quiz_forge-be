using QuizForge.Models;

namespace QuizForge.Repositories;

public interface IDocumentRepository
{
    Task CreateAsync(Document document, CancellationToken cancellationToken = default);

    Task<Document?> FindByFileHashSha256Async(string fileHashSha256, CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Document document,
        CancellationToken cancellationToken = default
    );

    Task<Document?> FindByIdAsync(long id, CancellationToken cancellationToken = default);
}
