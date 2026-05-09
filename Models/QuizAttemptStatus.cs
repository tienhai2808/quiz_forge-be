namespace QuizForge.Models;

public enum QuizAttemptStatus : byte
{
    InProgress = 1,
    Submitted = 2,
    Graded = 3,
    Expired = 4
}
