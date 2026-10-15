# Configuration

The limits and the summary labels come from configuration. `14`, `-90`, `60`, and the ten labels are no longer written in the lambdas.

This chapter's project is `ConfigurationWebApplication`. The weather routes match `03-Middleware`. The two `app.Use` delegates are not in this project.

## Lessons

| Lesson | Topic |
| --- | --- |
| `4.1.Options` | `IOptions<WeatherSettings>` supplies the Weather section |
| `4.2.Environment` | `appsettings.Development.json` replaces `MaxDays` |

## Files

| File | Role |
| --- | --- |
| `Program.cs` | Host, options registration, and routes |
| `WeatherSettings.cs` | The limits and the summary labels |
| `WeatherForecastStore.cs` | The list and the operations on it |
| `appsettings.json` | Logging, `AllowedHosts`, and the Weather section |
| `appsettings.Development.json` | Overrides `Weather:MaxDays` when `ASPNETCORE_ENVIRONMENT=Development` |
| `Properties/launchSettings.json` | URL and environment for local runs |
| `ConfigurationWebApplication.http` | Saved requests for the weather endpoints |

The project file only sets `net10.0`, nullable reference types, and implicit usings. It has no NuGet packages.

## Startup

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WeatherForecastStore>();
builder.Services.Configure<WeatherSettings>(builder.Configuration.GetSection("Weather"));
var app = builder.Build();

app.MapGet("/weatherforecast", (int days = 5, IOptions<WeatherSettings> settings) => { /* weather.MaxDays, weather.Summaries */ });

app.Run();
```

`CreateBuilder` loads `appsettings.json`, then `appsettings.{Environment}.json`. `Configure` binds the `Weather` section to `WeatherSettings` before `Build`. An endpoint asks for `IOptions<WeatherSettings>` the same way it asks for the store.

`GET /weatherforecast?days=2` reads the settings and does not ask for the store. POST, PUT, and PATCH read the settings for the temperature limits. GET by date and DELETE do not.

## Run

The `http` profile listens on `http://localhost:5122`. `launchBrowser` is `false`. The profile sets `ASPNETCORE_ENVIRONMENT` to `Development`.

Visual Studio: open `ConfigurationWebApplication.slnx`, select the `http` profile, and press F5.

```powershell
dotnet run --project ConfigurationWebApplication --launch-profile http
```

Then send a request in `ConfigurationWebApplication.http`.

Stop with Shift+F5, or Ctrl+C in the `dotnet run` terminal.

## Debug

Set a breakpoint on `var weather = settings.Value` in `MapGet("/weatherforecast")`. The requests to send are in `4.1.Options`.
