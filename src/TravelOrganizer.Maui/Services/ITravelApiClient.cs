using TravelOrganizer.Contracts;

namespace TravelOrganizer.Maui.Services;

public interface ITravelApiClient
{
    Task<TravelAdviceResponse> GetAdviceAsync(TravelAdviceRequest request, CancellationToken ct = default);
}

public class ApiException(string message) : Exception(message);
