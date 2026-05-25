using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using QuizForge.Exceptions;
using QuizForge.Options;

namespace QuizForge.Providers;

public class GcsProvider(
    IOptions<GcsOptions> gcsOptions,
    ILogger<GcsProvider> logger
) : IStorageProvider
{
    private readonly GcsOptions _gcsOptions = gcsOptions.Value;
    private readonly ILogger<GcsProvider> _logger = logger;
    private readonly Lazy<UrlSigner> _urlSigner = new(() =>
        UrlSigner.FromCredential(GoogleCredential.GetApplicationDefault()));

    public Task<string> GenerateUploadUrlAsync(
        string objectName,
        int expiresInMinutes,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(_gcsOptions.BucketName))
            throw new InternalServerException("Thiếu cấu hình Gcs:BucketName");

        if (expiresInMinutes <= 0)
            throw new BadRequestException("Thời gian hết hạn URL phải lớn hơn 0 phút");

        string url;
        try
        {
            url = _urlSigner.Value.Sign(
                _gcsOptions.BucketName,
                objectName,
                TimeSpan.FromMinutes(expiresInMinutes),
                HttpMethod.Put
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create presigned URL failed");
            throw new ExternalServiceException("Tạo presigned URL thất bại");
        }

        return Task.FromResult(url);
    }
}
