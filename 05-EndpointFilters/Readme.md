# Endpoint filters

A filter runs after the arguments are bound and before the route lambda. `TemperatureFilter` rejects a temperature outside the configured limits. POST, PUT, and PATCH no longer check it in the lambda.

This chapter's project is `EndpointFiltersWebApplication`. The weather routes match `04-Configuration`. Those routes are registered on one group. The Development settings are unchanged, so `MaxDays` is still 7.

## Lessons

| Lesson | Topic |
| --- | --- |
| `5.1.Filter` | `TemperatureFilter` returns 400 before the lambda |
| `5.2.Group` | `MapGroup` shares the prefix and the filter |

## Files

| File | Role |
| --- | --- |
| `Program.cs` | Host, options, the group, and routes |
| `TemperatureFilter.cs` | The temperature check for arguments on the group |
| `WeatherSettings.cs` | The limits and the summary labels |
| `WeatherForecastStore.cs` | The list and the operations on it |
| `appsettings.json` | Logging, `AllowedHosts`, and the Weather section |
| `appsettings.Development.json` | Overrides `Weather:MaxDays` when `ASPNETCORE_ENVIRONMENT=Development` |
| `Properties/launchSettings.json` | URL and environment for local runs |
| `EndpointFiltersWebApplication.http` | Saved requests for the weather endpoints |

The project file only sets `net10.0`, nullable reference types, and implicit usings. It has no NuGet packages.

## Startup

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WeatherForecastStore>();
builder.Services.Configure<WeatherSettings>(builder.Configuration.GetSection("Weather"));
var app = builder.Build();

var weather = app.MapGroup("/weatherforecast")
    .AddEndpointFilter<TemperatureFilter>();

weather.MapPost("/", (WeatherForecast forecast, WeatherForecastStore store) => { /* store.Save */ });

app.Run();
```

`AddEndpointFilter<TemperatureFilter>()` registers the filter on the group. A request constructs `TemperatureFilter`, enters `InvokeAsync`, then `next` enters the lambda. `GET /weatherforecast?days=2` still reads `IOptions<WeatherSettings>` for `MaxDays` and the labels. POST, PUT, and PATCH no longer take `IOptions<WeatherSettings>`.

## Run

The `http` profile listens on `http://localhost:5123`. `launchBrowser` is `false`.

Visual Studio: open `EndpointFiltersWebApplication.slnx`, select the `http` profile, and press F5.

```powershell
dotnet run --project EndpointFiltersWebApplication --launch-profile http
```

Then send a request in `EndpointFiltersWebApplication.http`.

Stop with Shift+F5, or Ctrl+C in the `dotnet run` terminal.

## Debug

Set a breakpoint on the first line of `InvokeAsync`, and another on the first line of `MapPost`. The requests to send are in `5.1.Filter`.
