using Microsoft.Extensions.Options;

public sealed class TemperatureFilter : IEndpointFilter
{
    private readonly WeatherSettings weather;

    public TemperatureFilter(IOptions<WeatherSettings> settings)
    {
        weather = settings.Value;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach (var argument in context.Arguments)
        {
            int? temperature = argument switch
            {
                WeatherForecast forecast => forecast.TemperatureC,
                WeatherForecastPatch patch => patch.TemperatureC,
                _ => null
            };

            if (temperature is not int value)
                continue;

            if (value < weather.MinTemperatureC || value > weather.MaxTemperatureC)
            {
                return Results.BadRequest(new
                {
                    error = $"temperatureC must be between {weather.MinTemperatureC} and {weather.MaxTemperatureC}."
                });
            }
        }

        return await next(context);
    }
}
