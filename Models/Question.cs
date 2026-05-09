namespace QuizForge.Models;

public class Question
{
    public long Id { get; set; }
    public long QuizId { get; set; }
    public int OrderNo { get; set; }
    public string Content { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    
    public Quiz Quiz { get; set; } = null!;
    public ICollection<QuestionOption> QuestionOptions { get; set; } = [];
    public ICollection<AttemptAnswer> AttemptAnswers { get; set; } = [];
}
