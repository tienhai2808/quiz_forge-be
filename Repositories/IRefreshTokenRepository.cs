using QuizForge.Models;

namespace QuizForge.Repositories;

public interface IRefreshTokenRepository
{
    Task CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<RefreshToken?> FindByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task DeleteAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
}
