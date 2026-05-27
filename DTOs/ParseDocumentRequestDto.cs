using System.Text.Json.Serialization;

namespace QuizForge.DTOs;

public sealed record ParseDocumentRequestDto(
    [property: JsonPropertyName("file_key")] string FileKey,
    [property: JsonPropertyName("backend")] string Backend,
    [property: JsonPropertyName("parse_method")] string ParseMethod
);