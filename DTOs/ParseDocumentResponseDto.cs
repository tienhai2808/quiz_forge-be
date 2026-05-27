using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuizForge.DTOs;

public sealed record ParseDocumentResponseDto(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("task_id")] string TaskId,
    [property: JsonPropertyName("backend")] string Backend,
    [property: JsonPropertyName("requested_backend")] string? RequestedBackend,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("file_name")] string? FileName,
    [property: JsonPropertyName("result")] ParseDocumentResultDto? Result,
    [property: JsonPropertyName("warnings")] IReadOnlyList<string>? Warnings,
    [property: JsonPropertyName("error")] string? Error
);

public sealed record ParseDocumentResultDto(
    [property: JsonPropertyName("result_type")] string ResultType,
    [property: JsonPropertyName("data")] JsonElement Data
);
