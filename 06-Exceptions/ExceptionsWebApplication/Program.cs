using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WeatherForecastStore>();
builder.Services.Configure<WeatherSettings>(builder.Configuration.GetSection("Weather"));
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
    try
    {
        await next(context);
    }
    catch (Exception exception)
    {
        app.Logger.LogError(exception, "the request failed.");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { error = "the request failed." });
    }
});

app.MapGet("/crash", () =>
{
    throw new InvalidOperationException("the forecast store failed.");
});

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

weather.MapPost("/", (WeatherForecast forecast, WeatherForecastStore store) =>
{
    store.Save(forecast);
    return Results.Created($"/weatherforecast/{forecast.Date:yyyy-MM-dd}", forecast);
});

weather.MapPut("/{date}", (DateOnly date, WeatherForecast forecast, WeatherForecastStore store) =>
{
    if (forecast.Date != date)
        return Results.BadRequest(new { error = "date in the body must match the URL." });

    if (!store.Replace(date, forecast))
        return Results.NotFound();

    return Results.Ok(forecast);
});

weather.MapPatch("/{date}", (DateOnly date, WeatherForecastPatch patch, WeatherForecastStore store) =>
{
    var updated = store.ApplyPatch(date, patch);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
});

weather.MapDelete("/{date}", (DateOnly date, WeatherForecastStore store) =>
    store.Remove(date) ? Results.NoContent() : Results.NotFound());

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

internal record WeatherForecastPatch(int? TemperatureC, string? Summary);
