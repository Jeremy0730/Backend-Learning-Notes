# CORS

A browser asks this API for permission before a page on another origin can read the response. The allowed origin is `http://localhost:5173`. The policy is not a substitute for the API key: a write still requires `writer-key`.

This chapter's project is `CorsWebApplication`. The routes, the `Writer` policy, and the write limit match `08-RateLimiting`.

## Lessons

| Lesson | Topic |
| --- | --- |
| `9.1.Preflight` | `OPTIONS` is answered before the route lambda |
| `9.2.Origin` | The allowed origin is a scheme, host, and port |

## Files

| File | Role |
| --- | --- |
| `Program.cs` | Host, CORS, auth, the limiter, and routes |
| `ApiKeyAuthHandler.cs` | Reads `X-Api-Key` and builds the principal |
| `AuthSettings.cs` | The reader key and the two writer keys |
| `TemperatureFilter.cs` | The temperature check for arguments on the group |
| `WeatherSettings.cs` | The limits and the summary labels |
| `WeatherForecastStore.cs` | The list and the operations on it |
| `appsettings.json` | Logging, `AllowedHosts`, and the Weather section |
| `appsettings.Development.json` | Local keys, including `WriterBKey` |
| `Properties/launchSettings.json` | URL and environment for local runs |
| `CorsWebApplication.http` | Saved browser-style requests and a write |

The project file only sets `net10.0`, nullable reference types, and implicit usings. It has no NuGet packages.

## Startup

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("ForecastUi", policy =>
        policy.WithOrigins("http://localhost:5173")
            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
            .WithHeaders("Content-Type", "X-Api-Key"));
});
var app = builder.Build();

app.UseCors("ForecastUi");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
```

`UseCors` is registered first. An `OPTIONS` preflight is answered there. A normal `GET` or `POST` continues, and the allow-origin header is applied to that response when the origin matches.

## Run

The `http` profile listens on `http://localhost:5127`. `launchBrowser` is `false`.

Visual Studio: open `CorsWebApplication.slnx`, select the `http` profile, and press F5.

```powershell
dotnet run --project CorsWebApplication --launch-profile http
```

Then send a request in `CorsWebApplication.http`.

Stop with Shift+F5, or Ctrl+C in the `dotnet run` terminal.

## Debug

Set a breakpoint on the first line of `MapPost`. The request to send is the `OPTIONS` call in `9.1.Preflight`.
