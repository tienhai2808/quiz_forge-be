namespace QuizForge.Providers;

public interface ITokenProvider
{
    string GenerateAccessToken(long userID);
}
