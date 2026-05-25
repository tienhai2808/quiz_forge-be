using QuizForge.DTOs;

namespace QuizForge.Services;

public interface IDocumentService
{
    Task<long> CreateAsync(
        CreateDocumentRequestDto dto, 
        long userId,
        CancellationToken cancellationToken = default
    );

    Task<UploadDocumentResponseDto> UploadAsync(
        UploadDocumentRequestDto dto,
        long userId,
        CancellationToken cancellationToken = default
    );
}