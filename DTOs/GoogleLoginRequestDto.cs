using System.ComponentModel.DataAnnotations;

namespace QuizForge.DTOs;

public class GoogleLoginRequestDto
{
    [Required(ErrorMessage = "Yêu cầu code")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yêu cầu uri chuyển hướng")]
    public string RedirectUri { get; set; } = string.Empty;
}
