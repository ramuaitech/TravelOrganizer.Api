using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using TravelOrganizer.Contracts;

namespace TravelOrganizer.Api.Services;

public class WeatherService(HttpClient http, IMemoryCache cache)
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public async Task<WeatherSnapshot?> GetCurrentAsync(double latitude, double longitude, CancellationToken ct = default)
    {
        var key = string.Create(CultureInfo.InvariantCulture,
            $"weather:{Math.Round(latitude, 2)}:{Math.Round(longitude, 2)}");

        if (cache.TryGetValue(key, out WeatherSnapshot? cached))
            return cached;

        // Invariant culture so decimals are never formatted with a comma.
        var url = string.Create(CultureInfo.InvariantCulture,
            $"v1/forecast?latitude={latitude}&longitude={longitude}" +
            $"&current=temperature_2m,relative_humidity_2m,wind_speed_10m&timezone=auto");

        var raw = await http.GetFromJsonAsync<OpenMeteoResponse>(url, ct);
        if (raw?.Current is null) return null;

        var snapshot = new WeatherSnapshot(raw.Current.Temperature, raw.Current.Humidity, raw.Current.WindSpeed);
        cache.Set(key, snapshot, CacheDuration);
        return snapshot;
    }

    private record OpenMeteoResponse([property: JsonPropertyName("current")] OpenMeteoCurrent? Current);

    private record OpenMeteoCurrent(
        [property: JsonPropertyName("temperature_2m")] double Temperature,
        [property: JsonPropertyName("relative_humidity_2m")] double Humidity,
        [property: JsonPropertyName("wind_speed_10m")] double WindSpeed);
}
