# Authentication

A request can name the caller. `X-Api-Key` is checked against two keys from configuration. The match becomes a `ClaimsPrincipal`. Write routes require the `writer` role. Read routes stay open.

This chapter's project is `AuthenticationWebApplication`. The weather group matches `05-EndpointFilters`. `GET /crash` is not in this project.

## Lessons

| Lesson | Topic |
| --- | --- |
| `7.1.Scheme` | `X-Api-Key` becomes a principal |
| `7.2.Policy` | Write routes require the `writer` role |

## Files

| File | Role |
| --- | --- |
| `Program.cs` | Host, auth registration, `/whoami`, and routes |
| `ApiKeyAuthHandler.cs` | Reads `X-Api-Key` and builds the principal |
| `AuthSettings.cs` | The reader key and the writer key |
| `TemperatureFilter.cs` | The temperature check for arguments on the group |
| `WeatherSettings.cs` | The limits and the summary labels |
| `WeatherForecastStore.cs` | The list and the operations on it |
| `appsettings.json` | Logging, `AllowedHosts`, and the Weather section |
| `appsettings.Development.json` | Local keys, and `Weather:MaxDays` in Development |
| `Properties/launchSettings.json` | URL and environment for local runs |
| `AuthenticationWebApplication.http` | Saved requests for whoami and the weather endpoints |

The project file only sets `net10.0`, nullable reference types, and implicit usings. It has no NuGet packages.

## Startup

```csharp
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));
builder.Services.AddAuthentication(ApiKeyAuthHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthHandler>(ApiKeyAuthHandler.SchemeName, _ => { });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Writer", policy =>
        policy.RequireAuthenticatedUser()
            .RequireClaim("role", "writer"));
});
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/whoami", (ClaimsPrincipal user) =>
    Results.Ok(new { name = user.Identity?.Name ?? "anonymous" }));

var writes = weather.MapGroup("").RequireAuthorization("Writer");
```

`UseAuthentication` runs the handler. `UseAuthorization` runs after it and stops a write when the policy fails. `GET /weatherforecast?days=2` and `GET /whoami` have no policy. POST, PUT, PATCH, and DELETE are on `writes`.

The keys live in `appsettings.Development.json` because the `http` profile is Development. `appsettings.json` has no `Auth` section. An environment variable `Auth__WriterKey` replaces `Auth:WriterKey`, the same merge as `Weather:MaxDays` in `4.2.Environment`.

## Run

The `http` profile listens on `http://localhost:5125`. `launchBrowser` is `false`.

Visual Studio: open `AuthenticationWebApplication.slnx`, select the `http` profile, and press F5.

```powershell
dotnet run --project AuthenticationWebApplication --launch-profile http
```

Then send a request in `AuthenticationWebApplication.http`.

Stop with Shift+F5, or Ctrl+C in the `dotnet run` terminal.

## Debug

Set a breakpoint on the first line of `HandleAuthenticateAsync`. The requests to send are in `7.1.Scheme`.
