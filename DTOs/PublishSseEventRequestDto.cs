using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace QuizForge.DTOs;

public class PublishSseEventRequestDto
{
    [Required(ErrorMessage = "Tên event là bắt buộc")]
    [MaxLength(100, ErrorMessage = "Tên event tối đa 100 ký tự")]
    public string EventName { get; set; } = string.Empty;

    public JsonElement? Data { get; set; }
}
