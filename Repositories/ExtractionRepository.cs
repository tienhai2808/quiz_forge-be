using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuizForge.Data;
using QuizForge.Exceptions;
using QuizForge.Models;

namespace QuizForge.Repositories;

public class ExtractionRepository(AppDbContext db): IExtractionRepository
{
    private readonly AppDbContext _db = db;

    public async Task CreateAsync(Extraction extraction, CancellationToken cancellationToken = default)
    {
        _db.Extractions.Add(extraction);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException pg &&
            pg.SqlState == PostgresErrorCodes.UniqueViolation
        )
        {
            throw new ConflictException("Dữ liệu trích xuất đã tồn tại");
        }
    }
}
