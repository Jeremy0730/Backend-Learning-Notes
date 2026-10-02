var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WeatherForecastStore>();
var app = builder.Build();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast/{date}", (DateOnly date, WeatherForecastStore store) =>
{
    var forecast = store.Find(date);
    return forecast is null ? Results.NotFound() : Results.Ok(forecast);
});

app.MapPost("/weatherforecast", (WeatherForecast forecast, WeatherForecastStore store) =>
{
    if (forecast.TemperatureC is < -90 or > 60)
        return Results.BadRequest(new { error = "temperatureC must be between -90 and 60." });

    store.Save(forecast);
    return Results.Created($"/weatherforecast/{forecast.Date:yyyy-MM-dd}", forecast);
});

app.MapPut("/weatherforecast/{date}", (DateOnly date, WeatherForecast forecast, WeatherForecastStore store) =>
{
    if (forecast.Date != date)
        return Results.BadRequest(new { error = "date in the body must match the URL." });

    if (forecast.TemperatureC is < -90 or > 60)
        return Results.BadRequest(new { error = "temperatureC must be between -90 and 60." });

    if (!store.Replace(date, forecast))
        return Results.NotFound();

    return Results.Ok(forecast);
});

app.MapPatch("/weatherforecast/{date}", (DateOnly date, WeatherForecastPatch patch, WeatherForecastStore store) =>
{
    if (patch.TemperatureC is < -90 or > 60)
        return Results.BadRequest(new { error = "temperatureC must be between -90 and 60." });

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
