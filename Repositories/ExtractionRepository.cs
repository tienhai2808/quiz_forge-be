using Microsoft.EntityFrameworkCore;
using QuizForge.Data;
using QuizForge.Models;

namespace QuizForge.Repositories;

public class ExtractionRepository(AppDbContext db): IExtractionRepository
{
    private readonly AppDbContext _db = db;

    public Task<Extraction?> FindByHashSha256(string hashSha256, CancellationToken cancellationToken)
        => _db.Extractions.FirstOrDefaultAsync(x => x.HashSha256 == hashSha256, cancellationToken);

    public async Task CreateAsync(Extraction extraction, CancellationToken cancellationToken = default)
    {
        await _db.Extractions.AddAsync(extraction, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
