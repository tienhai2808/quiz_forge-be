using System.Text.Json;

namespace QuizForge.Models;

public class Extraction
{
    public string HashSha256 { get; set; } = string.Empty;
    public JsonElement RawJson { get; set; }

    public ICollection<Document> Documents { get; set; } = [];
}
