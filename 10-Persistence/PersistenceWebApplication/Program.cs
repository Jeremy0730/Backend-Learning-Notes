using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<WeatherSettings>(builder.Configuration.GetSection("Weather"));
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));

var fileName = builder.Configuration["Forecasts:File"] ?? "forecasts.db";
var dbPath = Path.IsPathRooted(fileName)
    ? fileName
    : Path.Combine(builder.Environment.ContentRootPath, fileName);
var connectionString = new SqliteConnectionStringBuilder
{
    DataSource = dbPath,
    Pooling = false
}.ToString();
builder.Services.AddDbContext<WeatherDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<WeatherForecastStore>();

builder.Services.AddAuthentication(ApiKeyAuthHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthHandler>(ApiKeyAuthHandler.SchemeName, _ => { });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Writer", policy =>
        policy.RequireAuthenticatedUser()
            .RequireClaim("role", "writer"));
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new { error = "too many requests." }, cancellationToken);
    };
    options.AddPolicy("writes", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.User.Identity?.Name ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 2,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("ForecastUi", policy =>
        policy.WithOrigins("http://localhost:5173")
            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
            .WithHeaders("Content-Type", "X-Api-Key"));
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors("ForecastUi");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

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

weather.MapGet("/{date}", async (DateOnly date, WeatherForecastStore store) =>
{
    var forecast = await store.Find(date);
    return forecast is null ? Results.NotFound() : Results.Ok(forecast);
});

var writes = weather.MapGroup("")
    .RequireAuthorization("Writer")
    .RequireRateLimiting("writes");

writes.MapPost("/", async (WeatherForecast forecast, WeatherForecastStore store) =>
{
    await store.Save(forecast);
    return Results.Created($"/weatherforecast/{forecast.Date:yyyy-MM-dd}", forecast);
});

writes.MapPut("/{date}", async (DateOnly date, WeatherForecast forecast, WeatherForecastStore store) =>
{
    if (forecast.Date != date)
        return Results.BadRequest(new { error = "date in the body must match the URL." });

    if (!await store.Replace(date, forecast))
        return Results.NotFound();

    return Results.Ok(forecast);
});

writes.MapPatch("/{date}", async (DateOnly date, WeatherForecastPatch patch, WeatherForecastStore store) =>
{
    var updated = await store.ApplyPatch(date, patch);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
});

writes.MapDelete("/{date}", async (DateOnly date, WeatherForecastStore store) =>
    await store.Remove(date) ? Results.NoContent() : Results.NotFound());

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

internal record WeatherForecastPatch(int? TemperatureC, string? Summary);
