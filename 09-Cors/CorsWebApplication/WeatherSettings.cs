public sealed class WeatherSettings
{
    public int MaxDays { get; set; } = 14;

    public int MinTemperatureC { get; set; } = -90;

    public int MaxTemperatureC { get; set; } = 60;

    public string[] Summaries { get; set; } =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];
}
