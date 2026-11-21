namespace TravelOrganizer.Contracts;

public record TravelAdviceRequest(string Destination, string Question);

public record WeatherSnapshot(double TemperatureC, double HumidityPercent, double WindSpeedKmh);

public record TravelAdviceResponse(string Location, WeatherSnapshot Weather, string Advice);
