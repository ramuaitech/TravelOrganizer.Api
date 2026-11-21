using System.Text.Json.Serialization;

namespace TravelOrganizer.Api.Services;

public record GeoLocation(string DisplayName, double Latitude, double Longitude);

public class GeocodingService(HttpClient http)
{
    public async Task<GeoLocation?> FindAsync(string name, CancellationToken ct = default)
    {
        var url = $"v1/search?name={Uri.EscapeDataString(name)}&count=1&language=en&format=json";
        var response = await http.GetFromJsonAsync<GeoResponse>(url, ct);
        var hit = response?.Results?.FirstOrDefault();
        if (hit is null) return null;

        var display = string.IsNullOrWhiteSpace(hit.Country) ? hit.Name : $"{hit.Name}, {hit.Country}";
        return new GeoLocation(display, hit.Latitude, hit.Longitude);
    }

    private record GeoResponse([property: JsonPropertyName("results")] List<GeoResult>? Results);

    private record GeoResult(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("country")] string? Country,
        [property: JsonPropertyName("latitude")] double Latitude,
        [property: JsonPropertyName("longitude")] double Longitude);
}
