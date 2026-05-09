namespace QuizForge.Models;

public class AttemptAnswer
{
    public long AttemptId { get; set; }
    public long QuestionId { get; set; }
    public long? QuestionOptionId { get; set; }
    public string? TextAnswer { get; set; }
    public bool IsCorrect { get; set; }

    public QuizAttempt QuizAttempt { get; set; } = null!;
    public Question Question { get; set; } = null!;
    public QuestionOption? QuestionOption { get; set; }
}
