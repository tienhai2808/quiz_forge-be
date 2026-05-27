using System.Text.Json.Serialization;

namespace QuizForge.DTOs;

public sealed record ExtractDocumentMessageDto(
    [property: JsonPropertyName("file_key")] string FileKey,
    [property: JsonPropertyName("hash_sha256")] string HashSha256
);
