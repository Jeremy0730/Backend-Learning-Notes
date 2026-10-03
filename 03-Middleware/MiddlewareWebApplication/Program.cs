var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WeatherForecastStore>();
var app = builder.Build();

app.Use(async (HttpContext context, RequestDelegate next) =>
{
    var method = context.Request.Method;
    var path = context.Request.Path.Value ?? "/";
    context.Response.Headers.Append("X-Request-Path", path);
    await next(context);
    var status = context.Response.StatusCode;
    app.Logger.LogInformation("{Method} {Path} finished with {Status}", method, path, status);
});

app.Use(async (HttpContext context, RequestDelegate next) =>
{
    if (context.Request.Path.StartsWithSegments("/weatherforecast/blocked"))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new { error = "this path is stopped by middleware." });
        return;
    }

    await next(context);
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

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
