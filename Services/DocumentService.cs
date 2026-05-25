using System.Text.Json;
using IdGen;
using QuizForge.DTOs;
using QuizForge.Exceptions;
using QuizForge.Models;
using QuizForge.Repositories;
using StackExchange.Redis;

namespace QuizForge.Services;

public class DocumentService(
    IIdGenerator<long> idGenerator,
    IDocumentRepository documentRepo,
    IConnectionMultiplexer cache
) : IDocumentService
{
    private readonly IIdGenerator<long> _idGenerator = idGenerator;
    private readonly IDocumentRepository _documentRepo = documentRepo;
    private readonly IDatabase _cache = cache.GetDatabase();
    private readonly string _prefixKeyUploadDocumentCache = "document_upload";

    public async Task<long> CreateAsync(
        CreateDocumentRequestDto dto,
        long userId,
        CancellationToken cancellationToken
    )
    {
        string? sourceType = dto.SourceType;
        string? fileKey = dto.FileKey;
        string? fileHashSha256 = dto.FileHashSha256;

        if (dto.UploadCode != null)
        {
            string? data = await _cache.StringGetAsync($"{_prefixKeyUploadDocumentCache}:{dto.UploadCode}");
            if (data == null)
                throw new NotFoundException("Không tìm thấy dữ liệu từ bộ nhớ đệm");

            var docObj = JsonSerializer.Deserialize<UploadDocumentCacheDto>(data)
                ?? throw new ConflictException("Dữ liệu từ bộ nhớ đệm không hợp lệ");

            if (docObj.UserId != userId)
                throw new ConflictException("Người dùng không hợp lệ");

            sourceType = docObj.SourceType;
            fileKey = docObj.FileKey;
            fileHashSha256 = docObj.FileHashSha256;
        }

        if (
            string.IsNullOrWhiteSpace(sourceType)
            || string.IsNullOrWhiteSpace(fileKey)
            || string.IsNullOrWhiteSpace(fileHashSha256)
        )
            throw new BadRequestException("Yêu cầu đủ dữ liệu gửi lên");

        var documentId = _idGenerator.CreateId();
        var document = new Document
        {
            Id = documentId,
            UserId = userId,
            SourceType = sourceType,
            FileKey = fileKey,
            FileHashSha256 = fileHashSha256,
            IsPublic = dto.IsPublic
        };

        await _documentRepo.CreateAsync(document, cancellationToken);
        return documentId;
    }

    public async Task<UploadDocumentResponseDto> UploadAsync(
        UploadDocumentRequestDto dto,
        long userId,
        CancellationToken cancellationToken
    )
    {
        var document = await _documentRepo.FindByFileHashSha256Async(dto.FileHashSha256, cancellationToken);
        if (document != null)
        {
            var uploadCode = Guid.NewGuid().ToString("D");
            var cacheKey = $"{_prefixKeyUploadDocumentCache}:{uploadCode}";
            var cacheValue = new UploadDocumentCacheDto(
                userId,
                document.SourceType,
                document.FileHashSha256,
                document.FileKey
            );

            await _cache.StringSetAsync(
                cacheKey,
                JsonSerializer.Serialize(cacheValue),
                expiry: TimeSpan.FromMinutes(15)
            );

            return new UploadDocumentResponseDto(
                NeedUpload: false,
                UploadCode: uploadCode,
                SourceType: document.SourceType,
                FileKey: document.FileKey,
                FileHashSha256: document.FileHashSha256
            );
        }

        return new UploadDocumentResponseDto(
            NeedUpload: true
        );
    }
}
