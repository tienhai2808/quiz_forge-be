namespace QuizForge.Models;

public class Rating
{
    public long QuizId { get; set; }
    public long UserId { get; set; }
    public RatingStars Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    public Quiz Quiz { get; set; } = null!;
    public User User { get; set; } = null!;
}
