using Microsoft.Extensions.Http.Resilience;
using TravelOrganizer.Api.Options;
using TravelOrganizer.Api.Services;
using TravelOrganizer.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AiOptions>(builder.Configuration.GetSection(AiOptions.SectionName));
builder.Services.AddMemoryCache();
builder.Services.AddProblemDetails();

// --- Rate limiting: protects both the API and the paid AI provider ---
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddFixedWindowLimiter("api", l =>
    {
        l.PermitLimit = 30;
        l.Window = TimeSpan.FromMinutes(1);
        l.QueueLimit = 0;
    });
});

// --- External APIs (timeouts, retries and circuit breaker via the standard resilience handler) ---
builder.Services.AddHttpClient<WeatherService>(c => c.BaseAddress = new Uri("https://api.open-meteo.com/"))
    .AddStandardResilienceHandler();
builder.Services.AddHttpClient<GeocodingService>(c => c.BaseAddress = new Uri("https://geocoding-api.open-meteo.com/"))
    .AddStandardResilienceHandler();

// --- AI provider (selected by configuration) ---
var ai = builder.Configuration.GetSection(AiOptions.SectionName).Get<AiOptions>() ?? new AiOptions();

void ConfigureLlmResilience(HttpStandardResilienceOptions o)
{
    o.AttemptTimeout.Timeout = TimeSpan.FromSeconds(45);
    o.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(90);
    o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(120);
    o.Retry.DisableForUnsafeHttpMethods(); // never replay a paid POST automatically
}

switch (ai.Provider.ToLowerInvariant())
{
    case "anthropic":
        builder.Services.AddHttpClient<IAiService, AnthropicAiService>(c =>
                c.BaseAddress = new Uri(ai.Endpoint ?? "https://api.anthropic.com/"))
            .AddStandardResilienceHandler(ConfigureLlmResilience);
        break;
    case "openai":
        builder.Services.AddHttpClient<IAiService, OpenAiCompatibleAiService>(c =>
                c.BaseAddress = new Uri(ai.Endpoint ?? "https://api.openai.com/v1/"))
            .AddStandardResilienceHandler(ConfigureLlmResilience);
        break;
    default:
        builder.Services.AddSingleton<IAiService, MockAiService>();
        break;
}

var app = builder.Build();

app.UseExceptionHandler();
app.UseRateLimiter();

static IResult Fail(int status, string detail) => Results.Problem(detail: detail, statusCode: status);

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

var api = app.MapGroup("/api").RequireRateLimiting("api");

api.MapGet("/weather", async (double latitude, double longitude, WeatherService weather, CancellationToken ct) =>
{
    if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
        return Fail(400, "Invalid geographical coordinates.");

    var snapshot = await weather.GetCurrentAsync(latitude, longitude, ct);
    return snapshot is null ? Fail(502, "Weather information unavailable.") : Results.Ok(snapshot);
});

api.MapPost("/travel/advice", async (
    TravelAdviceRequest request,
    GeocodingService geocoding,
    WeatherService weather,
    IAiService aiService,
    ILogger<Program> logger,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Destination) || request.Destination.Length > 100)
        return Fail(400, "Destination is required (max 100 characters).");
    if (string.IsNullOrWhiteSpace(request.Question) || request.Question.Length > 500)
        return Fail(400, "Question is required (max 500 characters).");

    try
    {
        // 1. Resolve the destination to coordinates.
        var location = await geocoding.FindAsync(request.Destination.Trim(), ct);
        if (location is null)
            return Fail(404, $"Could not find a destination called '{request.Destination.Trim()}'.");

        // 2. Retrieve live weather from the external API.
        var snapshot = await weather.GetCurrentAsync(location.Latitude, location.Longitude, ct);
        if (snapshot is null)
            return Fail(502, "Weather information unavailable.");

        // 3. Give the LLM validated, structured data instead of letting it guess.
        var prompt = PromptBuilder.Build(location.DisplayName, snapshot, request.Question.Trim());
        var advice = await aiService.GenerateAsync(prompt.System, prompt.User, ct);

        // 4. Return a contextual response to the MAUI client.
        return Results.Ok(new TravelAdviceResponse(location.DisplayName, snapshot, advice));
    }
    catch (Exception ex) when (ex is HttpRequestException or TimeoutException or InvalidOperationException)
    {
        logger.LogError(ex, "Upstream failure while generating travel advice.");
        return Fail(502, "A dependent service failed. Please try again.");
    }
});

app.Run();
