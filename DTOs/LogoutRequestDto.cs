using System.ComponentModel.DataAnnotations;

namespace QuizForge.DTOs;

public class LogoutRequestDto
{
    [Required(ErrorMessage = "Yêu cầu refresh token")]
    public string RefreshToken { get; set; } = string.Empty;
}