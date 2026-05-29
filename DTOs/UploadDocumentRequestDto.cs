using System.ComponentModel.DataAnnotations;

namespace QuizForge.DTOs;

public class UploadDocumentRequestDto
{
    [Required(ErrorMessage = "Yêu cầu file hash sha256")]
    public string FileHashSha256 { get; set; } = string.Empty;
}