using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using QuizForge.Exceptions;

namespace QuizForge.Providers;

public class GcsProvider(
    IConfiguration configuration,
    ILogger<GcsProvider> logger
) : IStorageProvider
{
    private readonly ILogger<GcsProvider> _logger = logger;
    private readonly string _bucketName = configuration.GetValue("Gcs:BucketName", string.Empty);
    private readonly Lazy<UrlSigner> _urlSigner = new(() =>
        UrlSigner.FromCredential(GoogleCredential.GetApplicationDefault()));

    public Task<string> GenerateUploadUrlAsync(
        string objectName,
        int expiresInMinutes = 15,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(_bucketName))
            throw new InternalServerException("Thiếu cấu hình Gcs:BucketName");

        if (expiresInMinutes <= 0)
            throw new ValidationException("Thời gian hết hạn URL phải lớn hơn 0 phút");

        string url;
        try
        {
            url = _urlSigner.Value.Sign(
                _bucketName,
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
