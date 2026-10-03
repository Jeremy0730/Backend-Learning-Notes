public sealed class WeatherForecastStore
{
    private readonly List<WeatherForecast> forecasts;

    public WeatherForecastStore()
    {
        forecasts = [];
    }

    internal WeatherForecast? Find(DateOnly date) =>
        forecasts.Find(item => item.Date == date);

    internal void Save(WeatherForecast forecast)
    {
        forecasts.RemoveAll(item => item.Date == forecast.Date);
        forecasts.Add(forecast);
    }

    internal bool Replace(DateOnly date, WeatherForecast forecast)
    {
        var index = forecasts.FindIndex(item => item.Date == date);
        if (index < 0)
            return false;

        forecasts[index] = forecast;
        return true;
    }

    internal WeatherForecast? ApplyPatch(DateOnly date, WeatherForecastPatch patch)
    {
        var index = forecasts.FindIndex(item => item.Date == date);
        if (index < 0)
            return null;

        var current = forecasts[index];
        var updated = current with
        {
            TemperatureC = patch.TemperatureC ?? current.TemperatureC,
            Summary = patch.Summary ?? current.Summary
        };
        forecasts[index] = updated;
        return updated;
    }

    internal bool Remove(DateOnly date) =>
        forecasts.RemoveAll(item => item.Date == date) > 0;
}
