using System.ComponentModel.DataAnnotations;
using QuizForge.Models;

namespace QuizForge.DTOs;

public class UploadDocumentRequestDto : IValidatableObject
{
    [Required(ErrorMessage = "Yêu cầu file hash sha256")]
    public string FileHashSha256 { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
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