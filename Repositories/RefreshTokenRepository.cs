using QuizForge.Data;
using QuizForge.Models;
using Microsoft.EntityFrameworkCore;

namespace QuizForge.Repositories;

public class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    private readonly AppDbContext _db = db;

    public async Task CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<RefreshToken?> FindByTokenAsync(
        string token,
        CancellationToken cancellationToken = default
    )
        => _db.RefreshTokens.FirstOrDefaultAsync(
            x => x.Token == token,
            cancellationToken
        );

    public async Task DeleteAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _db.RefreshTokens.Remove(refreshToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
