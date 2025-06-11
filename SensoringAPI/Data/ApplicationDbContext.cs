using Microsoft.EntityFrameworkCore;
using SensoringAPI.Models;

namespace SensoringAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<WasteDetection> WasteDetections { get; set; }
    public DbSet<WeatherData> WeatherData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the relationship 
        modelBuilder.Entity<WasteDetection>()
            .HasOne(wd => wd.WeatherData)
            .WithMany() // WeatherData can be referenced by multiple WasteDetections
            .HasForeignKey(wd => wd.WeatherId);

        modelBuilder.Entity<WasteDetection>()
            .OwnsOne(wd => wd.Location);

        base.OnModelCreating(modelBuilder);
    }
}
