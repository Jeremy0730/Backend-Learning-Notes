using System.Security.Claims;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WeatherForecastStore>();
builder.Services.Configure<WeatherSettings>(builder.Configuration.GetSection("Weather"));
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));
builder.Services.AddAuthentication(ApiKeyAuthHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthHandler>(ApiKeyAuthHandler.SchemeName, _ => { });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Writer", policy =>
        policy.RequireAuthenticatedUser()
            .RequireClaim("role", "writer"));
});
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/whoami", (ClaimsPrincipal user) =>
    Results.Ok(new { name = user.Identity?.Name ?? "anonymous" }));

var weather = app.MapGroup("/weatherforecast")
    .AddEndpointFilter<TemperatureFilter>();

weather.MapGet("/", (int days = 5, IOptions<WeatherSettings> settings) =>
{
    var limits = settings.Value;
    if (days < 1 || days > limits.MaxDays)
    {
        return Results.BadRequest(new { error = $"days must be between 1 and {limits.MaxDays}." });
    }

    var forecast = Enumerable.Range(1, days).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            limits.Summaries[Random.Shared.Next(limits.Summaries.Length)]
        ))
        .ToArray();
    return Results.Ok(forecast);
});

weather.MapGet("/{date}", (DateOnly date, WeatherForecastStore store) =>
{
    var forecast = store.Find(date);
    return forecast is null ? Results.NotFound() : Results.Ok(forecast);
});

var writes = weather.MapGroup("").RequireAuthorization("Writer");

writes.MapPost("/", (WeatherForecast forecast, WeatherForecastStore store) =>
{
    store.Save(forecast);
    return Results.Created($"/weatherforecast/{forecast.Date:yyyy-MM-dd}", forecast);
});

writes.MapPut("/{date}", (DateOnly date, WeatherForecast forecast, WeatherForecastStore store) =>
{
    if (forecast.Date != date)
        return Results.BadRequest(new { error = "date in the body must match the URL." });

    if (!store.Replace(date, forecast))
        return Results.NotFound();

    return Results.Ok(forecast);
});

writes.MapPatch("/{date}", (DateOnly date, WeatherForecastPatch patch, WeatherForecastStore store) =>
{
    var updated = store.ApplyPatch(date, patch);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
});

writes.MapDelete("/{date}", (DateOnly date, WeatherForecastStore store) =>
    store.Remove(date) ? Results.NoContent() : Results.NotFound());

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

internal record WeatherForecastPatch(int? TemperatureC, string? Summary);
