using QuizForge.Models;

namespace QuizForge.Repositories;

public interface IUserRepository
{
    Task<User?> FindByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default);
    Task<User?> FindByIdAsync(long id, CancellationToken cancellationToken = default);
    Task CreateAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
