using QuizForge.DTOs;

namespace QuizForge.Services;

public interface IFileService
{
    Task<List<UploadPresignedUrlResponseDto>> CreatePresignedUrlsAsync(
        UploadPresignedUrlsRequestDto dto,
        CancellationToken cancellationToken = default
    );
}
