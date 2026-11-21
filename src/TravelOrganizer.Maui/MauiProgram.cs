using TravelOrganizer.Maui.Services;
using TravelOrganizer.Maui.ViewModels;
using TravelOrganizer.Maui.Views;

namespace TravelOrganizer.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        // The app only ever talks to OUR backend. Third-party APIs and AI keys stay server-side.
        builder.Services.AddHttpClient<ITravelApiClient, TravelApiClient>(client =>
        {
            client.BaseAddress = new Uri(ApiConfig.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        builder.Services.AddTransient<TravelAdviceViewModel>();
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}
