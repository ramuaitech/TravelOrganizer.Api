using TravelOrganizer.Api.Services;
using TravelOrganizer.Contracts;
using Xunit;

namespace TravelOrganizer.Api.Tests;

public class PromptBuilderTests
{
    private static readonly WeatherSnapshot Weather = new(21.5, 40, 12.3);

    [Fact]
    public void User_prompt_contains_live_weather_and_question()
    {
        var prompt = PromptBuilder.Build("Denver, United States", Weather, "Should I pack a jacket?");

        Assert.Contains("Denver, United States", prompt.User);
        Assert.Contains("21.5 °C", prompt.User);
        Assert.Contains("40%", prompt.User);
        Assert.Contains("12.3 km/h", prompt.User);
        Assert.Contains("Should I pack a jacket?", prompt.User);
    }

    [Fact]
    public void System_prompt_forbids_inventing_weather()
    {
        var prompt = PromptBuilder.Build("Denver", Weather, "Weekend trip?");

        Assert.Contains("Do not invent weather information", prompt.System);
    }
}
