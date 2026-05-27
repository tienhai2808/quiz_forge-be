using QuizForge.Models;

namespace QuizForge.Repositories;

public interface IExtractionRepository
{
    Task<Extraction?> FindByHashSha256(string hashSha256, CancellationToken cancellationToken = default);
    Task CreateAsync(Extraction extraction, CancellationToken cancellationToken = default);
}
