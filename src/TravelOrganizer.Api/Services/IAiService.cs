namespace TravelOrganizer.Api.Services;

public interface IAiService
{
    Task<string> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken ct = default);
}
