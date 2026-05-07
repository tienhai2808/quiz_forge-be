using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuizForge.Data;
using QuizForge.Exceptions;
using QuizForge.Models;

namespace QuizForge.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    private readonly AppDbContext _db = db;

    public Task<User?> FindByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
        => _db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId, cancellationToken);

    public Task<User?> FindByIdAsync(long id, CancellationToken cancellationToken = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        _db.Users.Add(user);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException pg &&
            pg.SqlState == PostgresErrorCodes.UniqueViolation
        )
        {
            throw new ConflictException("Nguời dùng đã tồn tại");
        }
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
