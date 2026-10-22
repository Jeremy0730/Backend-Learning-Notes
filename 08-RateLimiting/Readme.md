# Rate limiting

A writer may store two forecasts in one minute. The third write in that window is 429. The partition key is the caller's name, so `writer` and `writer-b` each have their own count.

This chapter's project is `RateLimitingWebApplication`. The routes and the `Writer` policy match `07-Authentication`. `writer-b-key` is a second writer.

## Lessons

| Lesson | Topic |
| --- | --- |
| `8.1.Window` | Two writes succeed, and the third is 429 |
| `8.2.Partition` | `writer-b` has a separate count |

## Files

| File | Role |
| --- | --- |
| `Program.cs` | Host, auth, the limiter, and routes |
| `ApiKeyAuthHandler.cs` | Reads `X-Api-Key` and builds the principal |
| `AuthSettings.cs` | The reader key and the two writer keys |
| `TemperatureFilter.cs` | The temperature check for arguments on the group |
| `WeatherSettings.cs` | The limits and the summary labels |
| `WeatherForecastStore.cs` | The list and the operations on it |
| `appsettings.json` | Logging, `AllowedHosts`, and the Weather section |
| `appsettings.Development.json` | Local keys, including `WriterBKey` |
| `Properties/launchSettings.json` | URL and environment for local runs |
| `RateLimitingWebApplication.http` | Saved requests for the write limit |

The project file only sets `net10.0`, nullable reference types, and implicit usings. It has no NuGet packages.

## Startup

```csharp
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

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

var writes = weather.MapGroup("")
    .RequireAuthorization("Writer")
    .RequireRateLimiting("writes");
```

The limiter runs after authorization. A 401 or 403 returns before a permit is taken. The window is one minute from the first counted write. Restart the app before sending the file again, or wait for the window to pass.

`appsettings.Development.json` sets `WriterBKey` to `writer-b-key`. That key's name is `writer-b` and its role is `writer`.

## Run

The `http` profile listens on `http://localhost:5126`. `launchBrowser` is `false`.

Visual Studio: open `RateLimitingWebApplication.slnx`, select the `http` profile, and press F5.

```powershell
dotnet run --project RateLimitingWebApplication --launch-profile http
```

Then send the requests in `RateLimitingWebApplication.http` from the top.

Stop with Shift+F5, or Ctrl+C in the `dotnet run` terminal.

## Debug

Set a breakpoint on `WriteAsJsonAsync` inside `OnRejected`. The requests to send are in `8.1.Window`.
