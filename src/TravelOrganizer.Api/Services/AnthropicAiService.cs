using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TravelOrganizer.Api.Options;

namespace TravelOrganizer.Api.Services;

public class AnthropicAiService(HttpClient http, IOptions<AiOptions> options) : IAiService
{
    public async Task<string> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
    {
        var cfg = options.Value;
        if (string.IsNullOrWhiteSpace(cfg.ApiKey))
            throw new InvalidOperationException("Ai:ApiKey is not configured.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/messages");
        request.Headers.Add("x-api-key", cfg.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = JsonContent.Create(new
        {
            model = cfg.Model,
            max_tokens = cfg.MaxTokens,
            system = systemPrompt,
            messages = new[] { new { role = "user", content = userPrompt } }
        });

        using var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var sb = new StringBuilder();
        foreach (var block in doc.RootElement.GetProperty("content").EnumerateArray())
        {
            if (block.GetProperty("type").GetString() == "text")
                sb.Append(block.GetProperty("text").GetString());
        }

        var text = sb.ToString().Trim();
        return text.Length > 0 ? text : throw new InvalidOperationException("The AI provider returned no text.");
    }
}
