using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TravelOrganizer.Contracts;
using TravelOrganizer.Maui.Services;

namespace TravelOrganizer.Maui.ViewModels;

public partial class TravelAdviceViewModel(ITravelApiClient api) : ObservableObject
{
    [ObservableProperty] private string destination = "Denver";

    [ObservableProperty]
    private string question = "Plan a weekend trip and tell me whether I should carry warm clothes.";

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(GetAdviceCommand))]
    private bool isBusy;

    [ObservableProperty] private bool hasResult;
    [ObservableProperty] private string locationName = "";
    [ObservableProperty] private string weatherSummary = "";
    [ObservableProperty] private string advice = "";

    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasError))]
    private string error = "";

    public bool HasError => !string.IsNullOrEmpty(Error);

    [RelayCommand(CanExecute = nameof(CanGetAdvice))]
    private async Task GetAdviceAsync()
    {
        Error = "";

        if (string.IsNullOrWhiteSpace(Destination) || string.IsNullOrWhiteSpace(Question))
        {
            Error = "Please enter a destination and a question.";
            return;
        }

        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            Error = "No internet connection.";
            return;
        }

        try
        {
            IsBusy = true;
            HasResult = false;

            var result = await api.GetAdviceAsync(new TravelAdviceRequest(Destination.Trim(), Question.Trim()));

            var inv = CultureInfo.CurrentCulture;
            LocationName = result.Location;
            WeatherSummary =
                $"Temperature: {result.Weather.TemperatureC.ToString("0.#", inv)} °C\n" +
                $"Humidity: {result.Weather.HumidityPercent.ToString("0.#", inv)}%\n" +
                $"Wind: {result.Weather.WindSpeedKmh.ToString("0.#", inv)} km/h";
            Advice = result.Advice;
            HasResult = true;
        }
        catch (ApiException ex) { Error = ex.Message; }
        catch (HttpRequestException) { Error = "Couldn't reach the server."; }
        catch (TaskCanceledException) { Error = "The request timed out. Please try again."; }
        finally { IsBusy = false; }
    }

    private bool CanGetAdvice() => !IsBusy;
}
