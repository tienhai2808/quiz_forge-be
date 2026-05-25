namespace QuizForge.Models;

public class ExtractionCache
{
    public string FileHashSha256 { get; set; } = string.Empty;
    public string ParserMode { get; set; } = ExtractionParserModes.RegexText;
    public string ExtractorVersion { get; set; } = string.Empty;
    public long DocumentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastHitAt { get; set; }

    public Document Document { get; set; } = null!;
}
