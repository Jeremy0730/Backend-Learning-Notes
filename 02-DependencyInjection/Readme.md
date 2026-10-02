# Dependency injection

The host keeps a container of services. `Program.cs` registers a type. An endpoint asks for that type by declaring a parameter. The container creates the object and decides how long that instance lives.

The saved forecasts are no longer a `List<WeatherForecast>` closed over by the lambdas. `WeatherForecastStore` owns the list. This chapter's project is `DependencyInjectionWebApplication`.

## Lessons

| Lesson | Topic |
| --- | --- |
| `2.1.Singleton` | One store for the whole process |

Two other lifetimes come later:

- **Scoped** creates one instance for a single request.
- **Transient** creates a new instance each time an endpoint or constructor asks.

## Files

| File | Role |
| --- | --- |
| `Program.cs` | Host, service registration, and routes |
| `WeatherForecastStore.cs` | The list and the operations on it |
| `appsettings.json` | Logging and `AllowedHosts` |
| `appsettings.Development.json` | Overrides applied when `ASPNETCORE_ENVIRONMENT=Development` |
| `Properties/launchSettings.json` | URL and environment for local runs |
| `DependencyInjectionWebApplication.http` | Saved requests for the weather endpoints |

The project file only sets `net10.0`, nullable reference types, and implicit usings. It has no NuGet packages.

## Startup

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WeatherForecastStore>();
var app = builder.Build();

app.MapPost("/weatherforecast", (WeatherForecast forecast, WeatherForecastStore store) => { /* store.Save */ });

app.Run();
```

`CreateBuilder` loads configuration and logging and creates the container. `AddSingleton` registers the store before `Build`. A request that declares a `WeatherForecastStore` parameter receives the instance the container holds.

`GET /weatherforecast?days=2` does not ask for the store. POST stores, GET by date reads, PUT replaces, PATCH changes fields, and DELETE removes. Those routes behave as they did in `01-Basics`.

## Run

The `http` profile listens on `http://localhost:5120`. `launchBrowser` is `false`.

Visual Studio: open `DependencyInjectionWebApplication.slnx`, select the `http` profile, and press F5.

```powershell
dotnet run --project DependencyInjectionWebApplication --launch-profile http
```

Then send a request in `DependencyInjectionWebApplication.http`.

Stop with Shift+F5, or Ctrl+C in the `dotnet run` terminal.

## Debug

Set a breakpoint on `forecasts = []` in the `WeatherForecastStore` constructor, and another on the first line of `MapPost`. The requests to send are in `2.1.Singleton`.
