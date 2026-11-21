namespace TravelOrganizer.Api.Options;

public class AiOptions
{
    public const string SectionName = "Ai";

    /// <summary>Mock | Anthropic | OpenAI</summary>
    public string Provider { get; set; } = "Mock";
    public string? ApiKey { get; set; }
    public string Model { get; set; } = "";
    public int MaxTokens { get; set; } = 600;
    /// <summary>Optional base-URL override (must end with a slash).</summary>
    public string? Endpoint { get; set; }
}
