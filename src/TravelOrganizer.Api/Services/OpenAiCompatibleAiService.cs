using System.Text.Json;
using Microsoft.Extensions.Options;
using TravelOrganizer.Api.Options;

namespace TravelOrganizer.Api.Services;

/// <summary>Works with OpenAI and any provider exposing the chat-completions format.</summary>
public class OpenAiCompatibleAiService(HttpClient http, IOptions<AiOptions> options) : IAiService
{
    public async Task<string> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
    {
        var cfg = options.Value;
        if (string.IsNullOrWhiteSpace(cfg.ApiKey))
            throw new InvalidOperationException("Ai:ApiKey is not configured.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new("Bearer", cfg.ApiKey);
        request.Content = JsonContent.Create(new
        {
            model = cfg.Model,
            max_tokens = cfg.MaxTokens,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        });

        using var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var text = doc.RootElement.GetProperty("choices")[0]
            .GetProperty("message").GetProperty("content").GetString()?.Trim();

        return string.IsNullOrEmpty(text) ? throw new InvalidOperationException("The AI provider returned no text.") : text;
    }
}
