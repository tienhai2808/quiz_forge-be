namespace QuizForge.DTOs;

public sealed record UploadPresignedUrlResponseDto(
    string Url,
    string Key
);
