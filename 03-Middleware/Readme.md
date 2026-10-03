# Middleware

A request enters a pipeline. Each middleware can read the request, call the next stage, and then see the response. The matched endpoint is the last stage.

This chapter's project is `MiddlewareWebApplication`. The weather routes match `02-DependencyInjection`, without the trace endpoints. Two `app.Use` delegates run before those routes.

## Lessons

| Lesson | Topic |
| --- | --- |
| `3.1.Use` | One middleware around every request |
| `3.2.NoNext` | A middleware answers one path and does not call `next` |

## Files

| File | Role |
| --- | --- |
| `Program.cs` | Host, middleware, service registration, and routes |
| `WeatherForecastStore.cs` | The list and the operations on it |
| `appsettings.json` | Logging and `AllowedHosts` |
| `appsettings.Development.json` | Overrides applied when `ASPNETCORE_ENVIRONMENT=Development` |
| `Properties/launchSettings.json` | URL and environment for local runs |
| `MiddlewareWebApplication.http` | Saved requests for the weather endpoints |

The project file only sets `net10.0`, nullable reference types, and implicit usings. It has no NuGet packages.

## Startup

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WeatherForecastStore>();
var app = builder.Build();

app.Use(async (HttpContext context, RequestDelegate next) => { /* log around next */ });
app.Use(async (HttpContext context, RequestDelegate next) => { /* /weatherforecast/blocked returns 403 */ });

app.MapGet("/weatherforecast", (int days = 5) => { /* build forecasts */ });

app.Run();
```

`Use` is registered before the routes. A request enters that delegate, then `next` enters the lambda. `GET /weatherforecast?days=2` does not ask for the store. POST, GET by date, PUT, PATCH, and DELETE do. The middleware runs for all of them.

## Run

The `http` profile listens on `http://localhost:5121`. `launchBrowser` is `false`.

Visual Studio: open `MiddlewareWebApplication.slnx`, select the `http` profile, and press F5.

```powershell
dotnet run --project MiddlewareWebApplication --launch-profile http
```

Then send a request in `MiddlewareWebApplication.http`.

Stop with Shift+F5, or Ctrl+C in the `dotnet run` terminal.

## Debug

Set a breakpoint on the first line inside `Use`, and another on the first line of `MapGet("/weatherforecast")`. The requests to send are in `3.1.Use`.
