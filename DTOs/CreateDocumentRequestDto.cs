using System.ComponentModel.DataAnnotations;

namespace QuizForge.DTOs;

public class CreateDocumentRequestDto
{
    public string? UploadCode { get; set; }
    public string? FileKey { get; set; }
    public string? FileHashSha256 { get; set; }

    [Required(ErrorMessage = "Yêu cầu quyền truy cập tài liệu")]
    public bool IsPublic { get; set; }
}