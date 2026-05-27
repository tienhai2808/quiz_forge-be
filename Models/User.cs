namespace QuizForge.Models;

public class User
{
    public long Id { get; set; }
    public string GoogleId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Document> Documents { get; set; } = [];
    // public ICollection<QuizAttempt> QuizAttempts { get; set; } = [];
    // public ICollection<Rating> Ratings { get; set; } = [];
}
