namespace QuizForge.Providers;

public interface IJwtProvider
{
    string GenerateAccessToken(long userID);
}
