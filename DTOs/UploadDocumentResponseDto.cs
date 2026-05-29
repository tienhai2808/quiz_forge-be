namespace QuizForge.DTOs;

public sealed record UploadDocumentResponseDto(
    bool NeedUpload,
    string? UploadCode = null,
    string? FileKey = null,
    string? FileHashSha256 = null
);