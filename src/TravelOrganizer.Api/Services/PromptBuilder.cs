using System.Globalization;
using TravelOrganizer.Contracts;

namespace TravelOrganizer.Api.Services;

public record Prompt(string System, string User);

public static class PromptBuilder
{
    public static Prompt Build(string location, WeatherSnapshot weather, string question)
    {
        var inv = CultureInfo.InvariantCulture;

        var system = """
            You are a practical travel planning assistant.
            Use only the weather data supplied by the application.
            Do not invent weather information.
            Mention that current conditions may differ from the forecast for future travel dates.
            Keep the answer concise and actionable.
            """;

        var user = $"""
            Destination: {location}

            Current temperature: {weather.TemperatureC.ToString("0.#", inv)} °C
            Humidity: {weather.HumidityPercent.ToString("0.#", inv)}%
            Wind speed: {weather.WindSpeedKmh.ToString("0.#", inv)} km/h

            User request:
            {question}
            """;

        return new Prompt(system, user);
    }
}
