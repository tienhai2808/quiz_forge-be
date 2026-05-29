using QuizForge.Models;

namespace QuizForge.Repositories;

public interface IExtractionRepository
{
    Task CreateAsync(Extraction extraction, CancellationToken cancellationToken = default);
}
