namespace QuizForge.Models;

public class QuizAttempt
{
    public long Id { get; set; }
    public long QuizId { get; set; }
    public long UserId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int TotalCorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public string Status { get; set; } = QuizAttemptStatuses.Submitted;

    public Quiz Quiz { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<AttemptAnswer> Answers { get; set; } = [];
}
