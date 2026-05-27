namespace QuizForge.Models;

public class Extraction
{
    public string HashSha256 { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string RawJson { get; set; } = string.Empty;

    public ICollection<Document> Documents { get; set; } = [];
}
