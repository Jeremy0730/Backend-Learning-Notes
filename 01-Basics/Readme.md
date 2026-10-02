# SuperSimpleWebApplication

Minimal ASP.NET Core Web API on .NET 10. One file owns startup, the route, and the response type. No controllers, no OpenAPI package, HTTP only.

## Files

| File                             | Role                                                        |
| -------------------------------- | ----------------------------------------------------------- |
| `Program.cs`                     | Host, route, and `WeatherForecast` record                   |
| `appsettings.json`               | Logging and `AllowedHosts`                                  |
| `appsettings.Development.json`   | Overrides applied when `ASPNETCORE_ENVIRONMENT=Development` |
| `Properties/launchSettings.json` | URL and environment for local runs                          |
| `SuperSimpleWebApplication.http` | Saved requests for the weather endpoints                    |

The project file only sets `net10.0`, nullable reference types, and implicit usings. It has no NuGet packages.

## Startup

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/weatherforecast", (int days = 5) => { /* build forecasts */ });
app.MapGet("/weatherforecast/{date}", (DateOnly date) => { /* read one stored forecast */ });
app.MapPost("/weatherforecast", (WeatherForecast forecast) => { /* store one forecast */ });
app.MapPut("/weatherforecast/{date}", (DateOnly date, WeatherForecast forecast) => { /* replace one stored forecast */ });
app.MapPatch("/weatherforecast/{date}", (DateOnly date, WeatherForecastPatch patch) => { /* change fields present in the body */ });
app.MapDelete("/weatherforecast/{date}", (DateOnly date) => { /* remove one stored forecast */ });

app.Run();
```

`CreateBuilder` loads configuration and logging. `Build` creates the app. Each `Map*` call registers one endpoint. `app.Run()` starts Kestrel and blocks until the process stops.

`Program.cs` runs once at startup. Each request enters the lambda for its route and method. Posted forecasts live in a `List<WeatherForecast>` in that process. Stopping the app clears the list.

## Endpoints

`GET /weatherforecast?days=2` generates forecasts and does not read the list.

- `days` defaults to 5 and must be from 1 to 14.
- Dates are tomorrow onward, one per day.
- `TemperatureC` is a random integer in `-20..54`.
- `Summary` is a random label from `Freezing` to `Scorching`.

`POST /weatherforecast` stores one forecast from the JSON body. `GET /weatherforecast/2026-10-05` reads it back. `PUT /weatherforecast/2026-10-05` replaces it. `PATCH /weatherforecast/2026-10-05` changes only the fields in the body. `DELETE /weatherforecast/2026-10-05` removes it and returns 204. PUT, PATCH, and DELETE do not create a missing date.

`WeatherForecast` is an `internal record` at the bottom of `Program.cs`. `Date`, `TemperatureC`, and `Summary` come from the constructor. `TemperatureF` is computed from Celsius and is still included in the JSON.

C# property names are PascalCase. The JSON uses camelCase: `date`, `temperatureC`, `temperatureF`, `summary`.

## Run

The `http` profile listens on `http://localhost:5119`. `launchBrowser` is `false`.

Visual Studio: select the `http` profile and press F5.

```powershell
dotnet run --project SuperSimpleWebApplication --launch-profile http
```

Then open `http://localhost:5119/weatherforecast`, or send a request in `SuperSimpleWebApplication.http`.

Stop with Shift+F5, or Ctrl+C in the `dotnet run` terminal.

## Debug

Set a breakpoint inside an endpoint lambda and start with F5. Send one request. Inspect the bound arguments before the lambda returns. After `return`, the framework serializes the body and writes the response.

A later request does not enter the lines above the `Map*` calls again.