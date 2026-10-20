# Exceptions

An endpoint can throw. A middleware registered after the logger catches that exception and writes 500. The logger then reads that status, because the catch returns without throwing again.

This chapter's project is `ExceptionsWebApplication`. The weather group matches `05-EndpointFilters`. `GET /crash` is registered on the app, outside the group.

## Lessons

| Lesson | Topic |
| --- | --- |
| `6.1.Throw` | `GET /crash` throws, and the catch middleware writes 500 |
| `6.2.Order` | The logger is outside the catch, so it still records 500 |

## Files

| File | Role |
| --- | --- |
| `Program.cs` | Host, middleware, options, the group, and routes |
| `TemperatureFilter.cs` | The temperature check for arguments on the group |
| `WeatherSettings.cs` | The limits and the summary labels |
| `WeatherForecastStore.cs` | The list and the operations on it |
| `appsettings.json` | Logging, `AllowedHosts`, and the Weather section |
| `appsettings.Development.json` | Overrides `Weather:MaxDays` when `ASPNETCORE_ENVIRONMENT=Development` |
| `Properties/launchSettings.json` | URL and environment for local runs |
| `ExceptionsWebApplication.http` | Saved requests for the crash and weather endpoints |

The project file only sets `net10.0`, nullable reference types, and implicit usings. It has no NuGet packages.

## Startup

```csharp
var app = builder.Build();

app.Use(async (HttpContext context, RequestDelegate next) => { /* header, next, log status */ });
app.Use(async (HttpContext context, RequestDelegate next) => { /* catch, write 500 */ });

app.MapGet("/crash", () => { /* throw */ });

var weather = app.MapGroup("/weatherforecast")
    .AddEndpointFilter<TemperatureFilter>();

app.Run();
```

The two delegates run for `/crash` and for the weather routes. `TemperatureFilter` runs only for the group. `GET /crash` does not enter the filter.

## Run

The `http` profile listens on `http://localhost:5124`. `launchBrowser` is `false`.

Visual Studio: open `ExceptionsWebApplication.slnx`, select the `http` profile, and press F5.

```powershell
dotnet run --project ExceptionsWebApplication --launch-profile http
```

Then send a request in `ExceptionsWebApplication.http`.

Stop with Shift+F5, or Ctrl+C in the `dotnet run` terminal.

## Debug

Set a breakpoint on `throw`, and another on `app.Logger.LogError`. The request to send is in `6.1.Throw`.
