namespace QuizForge.DTOs;

public sealed record UploadDocumentCacheDto(
    long UserId,
    string SourceType,
    string FileHashSha256,
    string FileKey
);