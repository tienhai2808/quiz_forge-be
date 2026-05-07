namespace QuizForge.Models;

public class RefreshToken
{
    public string Token { get; set; } = string.Empty;
    public long UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public User User { get; set; } = null!;
}