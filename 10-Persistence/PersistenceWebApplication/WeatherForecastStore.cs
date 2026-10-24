using Microsoft.EntityFrameworkCore;

public sealed class WeatherForecastStore
{
    private readonly WeatherDbContext db;

    public WeatherForecastStore(WeatherDbContext db)
    {
        this.db = db;
    }

    internal async Task<WeatherForecast?> Find(DateOnly date)
    {
        var row = await db.Forecasts.FirstOrDefaultAsync(item => item.Date == date);
        return row is null ? null : ToForecast(row);
    }

    internal async Task Save(WeatherForecast forecast)
    {
        var row = await db.Forecasts.FirstOrDefaultAsync(item => item.Date == forecast.Date);
        if (row is null)
        {
            db.Forecasts.Add(new ForecastRow
            {
                Date = forecast.Date,
                TemperatureC = forecast.TemperatureC,
                Summary = forecast.Summary
            });
        }
        else
        {
            row.TemperatureC = forecast.TemperatureC;
            row.Summary = forecast.Summary;
        }

        await db.SaveChangesAsync();
    }

    internal async Task<bool> Replace(DateOnly date, WeatherForecast forecast)
    {
        var row = await db.Forecasts.FirstOrDefaultAsync(item => item.Date == date);
        if (row is null)
            return false;

        row.TemperatureC = forecast.TemperatureC;
        row.Summary = forecast.Summary;
        await db.SaveChangesAsync();
        return true;
    }

    internal async Task<WeatherForecast?> ApplyPatch(DateOnly date, WeatherForecastPatch patch)
    {
        var row = await db.Forecasts.FirstOrDefaultAsync(item => item.Date == date);
        if (row is null)
            return null;

        if (patch.TemperatureC is int temperature)
            row.TemperatureC = temperature;

        if (patch.Summary is not null)
            row.Summary = patch.Summary;

        await db.SaveChangesAsync();
        return ToForecast(row);
    }

    internal async Task<bool> Remove(DateOnly date)
    {
        var row = await db.Forecasts.FirstOrDefaultAsync(item => item.Date == date);
        if (row is null)
            return false;

        db.Forecasts.Remove(row);
        await db.SaveChangesAsync();
        return true;
    }

    private static WeatherForecast ToForecast(ForecastRow row) =>
        new(row.Date, row.TemperatureC, row.Summary);
}
