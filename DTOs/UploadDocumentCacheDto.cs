namespace QuizForge.DTOs;

public sealed record UploadDocumentCacheDto(
    long UserId,
    string FileHashSha256,
    string FileKey
);