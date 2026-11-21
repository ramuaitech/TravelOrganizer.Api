namespace TravelOrganizer.Api.Services;

/// <summary>Works without an API key so the whole pipeline can be exercised locally.</summary>
public class MockAiService : IAiService
{
    public Task<string> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken ct = default) =>
        Task.FromResult(
            "[Mock AI] No AI provider is configured, so this is a placeholder.\n" +
            "Set Ai:Provider (Anthropic or OpenAI) and Ai:ApiKey to get real recommendations.\n\n" +
            "Prompt that would be sent to the model:\n" + userPrompt);
}
