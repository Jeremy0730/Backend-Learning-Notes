using Microsoft.EntityFrameworkCore;

public sealed class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
        : base(options)
    {
    }

    public DbSet<ForecastRow> Forecasts => Set<ForecastRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ForecastRow>(entity =>
        {
            entity.ToTable("Forecasts");
            entity.HasKey(row => row.Date);
        });
    }
}
