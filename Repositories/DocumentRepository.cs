using Microsoft.EntityFrameworkCore;
using QuizForge.Data;
using QuizForge.Models;

namespace QuizForge.Repositories;

public class DocumentRepository(AppDbContext db) : IDocumentRepository
{
    private readonly AppDbContext _db = db;

    public async Task CreateAsync(Document document, CancellationToken cancellationToken)
    {
        _db.Documents.Add(document);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<Document?> FindByFileHashSha256Async(string fileHashSha256, CancellationToken cancellationToken)
        => _db.Documents.FirstOrDefaultAsync(d => d.FileHashSha256 == fileHashSha256, cancellationToken);

    public Task<Document?> FindByIdAsync(long id, CancellationToken cancellationToken)
        => _db.Documents.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task UpdateAsync(
        Document document,
        CancellationToken cancellationToken = default
    )
    {
        _db.Documents.Update(document);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
