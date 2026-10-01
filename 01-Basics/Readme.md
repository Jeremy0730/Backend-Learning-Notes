# SuperSimpleWebApplication

Minimal ASP.NET Core Web API on .NET 10. One file owns startup, the route, and the response type. No controllers, no OpenAPI package, HTTP only.

## Files

| File                             | Role                                                        |
| -------------------------------- | ----------------------------------------------------------- |
| `Program.cs`                     | Host, route, and `WeatherForecast` record                   |
| `appsettings.json`               | Logging and `AllowedHosts`                                  |
| `appsettings.Development.json`   | Overrides applied when `ASPNETCORE_ENVIRONMENT=Development` |
| `Properties/launchSettings.json` | URL and environment for local runs                          |
| `SuperSimpleWebApplication.http` | Saved `GET /weatherforecast` request                        |

The project file only sets `net10.0`, nullable reference types, and implicit usings. It has no NuGet packages.

## Startup

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/weatherforecast", () => { /* build 5 forecasts */ });

app.Run();
```

`CreateBuilder` loads configuration and logging. `Build` creates the app. `MapGet` registers one endpoint. `app.Run()` starts Kestrel and blocks until the process stops.

`Program.cs` runs once at startup. Each request enters the lambda passed to `MapGet`.

## The endpoint

`GET /weatherforecast` returns five `WeatherForecast` values as JSON.

- Dates are tomorrow through five days ahead.
- `TemperatureC` is a random integer in `-20..54`.
- `Summary` is a random label from `Freezing` to `Scorching`.

`WeatherForecast` is an `internal record` at the bottom of `Program.cs`. `Date`, `TemperatureC`, and `Summary` come from the constructor. `TemperatureF` is computed from Celsius and is still included in the JSON.

C# property names are PascalCase. The JSON uses camelCase: `date`, `temperatureC`, `temperatureF`, `summary`.

## Run

The `http` profile listens on `http://localhost:5119`. `launchBrowser` is `false`.

Visual Studio: select the `http` profile and press F5.

```powershell
dotnet run --project SuperSimpleWebApplication --launch-profile http
```

Then open `http://localhost:5119/weatherforecast`, or send the request in `SuperSimpleWebApplication.http`.

Stop with Shift+F5, or Ctrl+C in the `dotnet run` terminal.

## Debug

Set a breakpoint inside the `MapGet` lambda and start with F5. Send one GET. Inspect `forecast` before the lambda returns. After `return`, the framework serializes the array and writes the response.

A later request does not enter the lines above `MapGet` again.