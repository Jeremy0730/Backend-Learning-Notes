var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var forecasts = new List<WeatherForecast>();

app.MapGet("/weatherforecast", (int days = 5) =>
{
    const int maxDays = 14;
    if (days is < 1 or > maxDays)
    {
        return Results.BadRequest(new { error = $"days must be between 1 and {maxDays}." });
    }

    var forecast = Enumerable.Range(1, days).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return Results.Ok(forecast);
});

app.MapGet("/weatherforecast/{date}", (DateOnly date) =>
{
    var forecast = forecasts.Find(item => item.Date == date);
    return forecast is null ? Results.NotFound() : Results.Ok(forecast);
});

app.MapPost("/weatherforecast", (WeatherForecast forecast) =>
{
    if (forecast.TemperatureC is < -90 or > 60)
        return Results.BadRequest(new { error = "temperatureC must be between -90 and 60." });

    forecasts.RemoveAll(item => item.Date == forecast.Date);
    forecasts.Add(forecast);
    return Results.Created($"/weatherforecast/{forecast.Date:yyyy-MM-dd}", forecast);
});

app.MapPut("/weatherforecast/{date}", (DateOnly date, WeatherForecast forecast) =>
{
    if (forecast.Date != date)
        return Results.BadRequest(new { error = "date in the body must match the URL." });

    if (forecast.TemperatureC is < -90 or > 60)
        return Results.BadRequest(new { error = "temperatureC must be between -90 and 60." });

    var index = forecasts.FindIndex(item => item.Date == date);
    if (index < 0)
        return Results.NotFound();

    forecasts[index] = forecast;
    return Results.Ok(forecast);
});

app.MapPatch("/weatherforecast/{date}", (DateOnly date, WeatherForecastPatch patch) =>
{
    if (patch.TemperatureC is < -90 or > 60)
        return Results.BadRequest(new { error = "temperatureC must be between -90 and 60." });

    var index = forecasts.FindIndex(item => item.Date == date);
    if (index < 0)
        return Results.NotFound();

    var current = forecasts[index];
    var updated = current with
    {
        TemperatureC = patch.TemperatureC ?? current.TemperatureC,
        Summary = patch.Summary ?? current.Summary
    };
    forecasts[index] = updated;
    return Results.Ok(updated);
});

app.MapDelete("/weatherforecast/{date}", (DateOnly date) =>
{
    var removed = forecasts.RemoveAll(item => item.Date == date);
    return removed == 0 ? Results.NotFound() : Results.NoContent();
});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

internal record WeatherForecastPatch(int? TemperatureC, string? Summary);
