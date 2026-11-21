using System.Net.Http.Json;
using TravelOrganizer.Contracts;

namespace TravelOrganizer.Maui.Services;

public class TravelApiClient(HttpClient http) : ITravelApiClient
{
    private record ProblemDto(string? Title, string? Detail);

    public async Task<TravelAdviceResponse> GetAdviceAsync(TravelAdviceRequest request, CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync("api/travel/advice", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            ProblemDto? problem = null;
            try { problem = await response.Content.ReadFromJsonAsync<ProblemDto>(cancellationToken: ct); }
            catch { /* body was not a ProblemDetails document */ }

            throw new ApiException(problem?.Detail ?? $"Request failed ({(int)response.StatusCode}).");
        }

        return await response.Content.ReadFromJsonAsync<TravelAdviceResponse>(cancellationToken: ct)
               ?? throw new ApiException("The server returned an empty response.");
    }
}
