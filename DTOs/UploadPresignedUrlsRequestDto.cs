using System.ComponentModel.DataAnnotations;

namespace QuizForge.DTOs;

public class UploadPresignedUrlsRequestDto
{
    [Required(ErrorMessage = "Danh sách file là bắt buộc")]
    [MinLength(1, ErrorMessage = "Phải có ít nhất 1 file")]
    public List<UploadPresignedUrlRequestDto> Files { get; set; } = [];
}

public class UploadPresignedUrlRequestDto
{
    [Required(ErrorMessage = "Yêu cầu tên tệp")]
    [RegularExpression(@"\S+", ErrorMessage = "Tên tệp không hợp lệ")]
    public string FileName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yêu cầu loại tệp")]
    [RegularExpression(@"\S+", ErrorMessage = "Loại tệp không hợp lệ")]
    public string ContentType { get; set; } = string.Empty;
}