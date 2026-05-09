using System.ComponentModel.DataAnnotations;

namespace QuizForge.DTOs;

public class GoogleLoginRequestDto
{
    [Required(ErrorMessage = "Yêu cầu code")]
    [RegularExpression(@"\S+", ErrorMessage = "Code không hợp lệ")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yêu cầu uri chuyển hướng")]
    [RegularExpression(@"\S+", ErrorMessage = "Uri chuyển hướng không hợp lệ")]
    public string RedirectUri { get; set; } = string.Empty;
}
