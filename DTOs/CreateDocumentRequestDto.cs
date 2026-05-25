using System.ComponentModel.DataAnnotations;
using QuizForge.Models;

namespace QuizForge.DTOs;

public class CreateDocumentRequestDto : IValidatableObject
{
    public string? UploadCode { get; set; }

    public string? SourceType { get; set; }
    public string? FileKey { get; set; }
    public string? FileHashSha256 { get; set; }

    [Required(ErrorMessage = "Yêu cầu quyền truy cập tài liệu")]
    public bool IsPublic { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(SourceType))
        {
            yield break;
        }

        var validSourceTypes = new[]
        {
            DocumentSourceTypes.Docx,
            DocumentSourceTypes.PdfText,
            DocumentSourceTypes.PdfOcr,
            DocumentSourceTypes.Image
        };

        if (!validSourceTypes.Contains(SourceType, StringComparer.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                $"Loại tài liệu phải là 1 trong: {string.Join(", ", validSourceTypes)}",
                [nameof(SourceType)]
            );
        }
    }
}