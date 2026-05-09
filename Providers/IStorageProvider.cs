namespace QuizForge.Providers;

public interface IStorageProvider
{
    Task<string> GenerateUploadUrlAsync(
        string objectName,
        int expiresInMinutes = 15,
        CancellationToken cancellationToken = default
    );
}
