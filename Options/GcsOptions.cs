using System.ComponentModel.DataAnnotations;

namespace QuizForge.Options;

public record GcsOptions
{
    public const string SectionName = "Gcs";

    [Required]
    public required string BucketName { get; init; }
}
