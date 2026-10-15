using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WeatherForecastStore>();
builder.Services.Configure<WeatherSettings>(builder.Configuration.GetSection("Weather"));
var app = builder.Build();

app.MapGet("/weatherforecast", (int days = 5, IOptions<WeatherSettings> settings) =>
{
    var weather = settings.Value;
    if (days < 1 || days > weather.MaxDays)
    {
        return Results.BadRequest(new { error = $"days must be between 1 and {weather.MaxDays}." });
    }

    var forecast = Enumerable.Range(1, days).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            weather.Summaries[Random.Shared.Next(weather.Summaries.Length)]
        ))
        .ToArray();
    return Results.Ok(forecast);
});

app.MapGet("/weatherforecast/{date}", (DateOnly date, WeatherForecastStore store) =>
{
    var forecast = store.Find(date);
    return forecast is null ? Results.NotFound() : Results.Ok(forecast);
});

app.MapPost("/weatherforecast", (WeatherForecast forecast, WeatherForecastStore store, IOptions<WeatherSettings> settings) =>
{
    var weather = settings.Value;
    if (forecast.TemperatureC < weather.MinTemperatureC || forecast.TemperatureC > weather.MaxTemperatureC)
        return Results.BadRequest(new { error = $"temperatureC must be between {weather.MinTemperatureC} and {weather.MaxTemperatureC}." });

    store.Save(forecast);
    return Results.Created($"/weatherforecast/{forecast.Date:yyyy-MM-dd}", forecast);
});

app.MapPut("/weatherforecast/{date}", (DateOnly date, WeatherForecast forecast, WeatherForecastStore store, IOptions<WeatherSettings> settings) =>
{
    var weather = settings.Value;
    if (forecast.Date != date)
        return Results.BadRequest(new { error = "date in the body must match the URL." });

    if (forecast.TemperatureC < weather.MinTemperatureC || forecast.TemperatureC > weather.MaxTemperatureC)
        return Results.BadRequest(new { error = $"temperatureC must be between {weather.MinTemperatureC} and {weather.MaxTemperatureC}." });

    if (!store.Replace(date, forecast))
        return Results.NotFound();

    return Results.Ok(forecast);
});

app.MapPatch("/weatherforecast/{date}", (DateOnly date, WeatherForecastPatch patch, WeatherForecastStore store, IOptions<WeatherSettings> settings) =>
{
    var weather = settings.Value;
    if (patch.TemperatureC < weather.MinTemperatureC || patch.TemperatureC > weather.MaxTemperatureC)
        return Results.BadRequest(new { error = $"temperatureC must be between {weather.MinTemperatureC} and {weather.MaxTemperatureC}." });

    var updated = store.ApplyPatch(date, patch);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
});

app.MapDelete("/weatherforecast/{date}", (DateOnly date, WeatherForecastStore store) =>
    store.Remove(date) ? Results.NoContent() : Results.NotFound());

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

internal record WeatherForecastPatch(int? TemperatureC, string? Summary);
