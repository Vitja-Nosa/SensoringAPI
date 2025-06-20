using Microsoft.EntityFrameworkCore;
using SensoringAPI.Models;
using SensoringAPI.Services.Interfaces;

namespace SensoringAPI.Data;

public class ApplicationDbContext : DbContext
{
    private readonly ITestModeService _testModeService;


    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITestModeService testModeService)
        : base(options)
    {
        _testModeService = testModeService;
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

    public override int SaveChanges()
    {
        // Simulate saving to DB if testmode is on
        if (_testModeService.IsTestMode)
            return 0; 

        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Simulate saving to DB if testmode is on
        if (_testModeService.IsTestMode)
            return 0;

        return await base.SaveChangesAsync(cancellationToken);
    }

}
