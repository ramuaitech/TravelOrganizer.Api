namespace TravelOrganizer.Maui.Services;

public static class ApiConfig
{
    // Android emulator reaches the host machine through 10.0.2.2.
    // For a physical device, use your computer's LAN IP, or a deployed HTTPS URL.
    public static string BaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5080/"
            : "http://localhost:5080/";
}
