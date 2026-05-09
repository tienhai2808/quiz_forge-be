using QuizForge.DTOs;
using QuizForge.Exceptions;
using QuizForge.Providers;

namespace QuizForge.Services;

public class FileService(IStorageProvider storageProvider) : IFileService
{
    private readonly IStorageProvider _storageProvider = storageProvider;

    public async Task<List<UploadPresignedUrlResponseDto>> CreatePresignedUrlsAsync(
        UploadPresignedUrlsRequestDto dto,
        CancellationToken cancellationToken = default
    )
    {
        var results = new List<UploadPresignedUrlResponseDto>(dto.Files.Count);
        foreach (var file in dto.Files)
        {
            var safeFileName = Path.GetFileName(file.FileName);
            var objectName = $"uploads/{safeFileName}";
            var url = await _storageProvider.GenerateUploadUrlAsync(
                objectName,
                15,
                cancellationToken
            );

            results.Add(new UploadPresignedUrlResponseDto(
                Url: url,
                Key: objectName
            ));
        }

        return results;
    }
}
