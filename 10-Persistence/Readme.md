# Persistence

The forecasts are rows in a SQLite file. `WeatherForecastStore` is scoped and takes a `WeatherDbContext`. Stopping the process no longer clears the table.

This chapter's project is `PersistenceWebApplication`. The routes, the writer policy, the write limit, and the CORS policy match `09-Cors`. The project references `Microsoft.EntityFrameworkCore.Sqlite`.

## Lessons

| Lesson | Topic |
| --- | --- |
| `10.1.Context` | A scoped context writes one row per date |
| `10.2.File` | The row is still there after a restart |

## Files

| File | Role |
| --- | --- |
| `Program.cs` | Host, the database path, and routes |
| `WeatherDbContext.cs` | The context and the `Forecasts` table |
| `ForecastRow.cs` | One stored forecast |
| `WeatherForecastStore.cs` | Find, save, replace, patch, and remove through the context |
| `ApiKeyAuthHandler.cs` | Reads `X-Api-Key` and builds the principal |
| `AuthSettings.cs` | The reader key and the two writer keys |
| `TemperatureFilter.cs` | The temperature check for arguments on the group |
| `WeatherSettings.cs` | The limits and the summary labels |
| `appsettings.json` | Logging, `AllowedHosts`, `Forecasts:File`, and the Weather section |
| `appsettings.Development.json` | Local keys, and `Weather:MaxDays` in Development |
| `Properties/launchSettings.json` | URL and environment for local runs |
| `PersistenceWebApplication.http` | A writer POST and the GET that reads it back |

## Startup

```csharp
var fileName = builder.Configuration["Forecasts:File"] ?? "forecasts.db";
var dbPath = Path.IsPathRooted(fileName)
    ? fileName
    : Path.Combine(builder.Environment.ContentRootPath, fileName);
var connectionString = new SqliteConnectionStringBuilder
{
    DataSource = dbPath,
    Pooling = false
}.ToString();
builder.Services.AddDbContext<WeatherDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<WeatherForecastStore>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
    db.Database.EnsureCreated();
}
```

`Forecasts:File` is `forecasts.db`. A relative name is placed in the project folder. `AddDbContext` registers a scoped context. The store takes that context, so the store is scoped too. `EnsureCreated` builds the table once, before `app.Run()`. `Pooling` is off, so the file is released when the last context is disposed.

`GET /weatherforecast?days=2` still builds forecasts in memory and does not ask for the store. POST, GET by date, PUT, PATCH, and DELETE await the store. The table holds `Date`, `TemperatureC`, and `Summary`. `TemperatureF` is still calculated on the record.

## Run

The `http` profile listens on `http://localhost:5128`. `launchBrowser` is `false`.

Visual Studio: open `PersistenceWebApplication.slnx`, select the `http` profile, and press F5.

```powershell
dotnet run --project PersistenceWebApplication --launch-profile http
```

Then send a request in `PersistenceWebApplication.http`.

Stop with Shift+F5, or Ctrl+C in the `dotnet run` terminal.

## Debug

Set a breakpoint on `db.Database.EnsureCreated()`, and another in the `WeatherForecastStore` constructor. The requests to send are in `10.1.Context`.
