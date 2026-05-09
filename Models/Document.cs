namespace QuizForge.Models;

public class Document
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public DocumentSourceType SourceType { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileHashSha256 { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsPublic { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Quiz> Quizzes { get; set; } = [];
}
