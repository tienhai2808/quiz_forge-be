namespace QuizForge.Models;

public class Document
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string FileKey { get; set; } = string.Empty;
    public string? FileHashSha256 { get; set; }
    public string Status { get; set; } = DocumentStatuses.Processing;
    public DateTime CreatedAt { get; set; }
    public bool IsPublic { get; set; }

    public User User { get; set; } = null!;
    public Extraction? Extraction { get; set; }
    // public ICollection<Quiz> Quizzes { get; set; } = [];
}
