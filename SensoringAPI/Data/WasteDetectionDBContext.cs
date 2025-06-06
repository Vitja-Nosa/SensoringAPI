using Microsoft.EntityFrameworkCore;

namespace SensoringAPI.Data;

public class WasteDetectionDBContext : DbContext
{
    public WasteDetectionDBContext(DbContextOptions<WasteDetectionDBContext> options)
        : base(options)
    {
    }

    public DbSet<Models.WasteDetection> WasteDetections { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.WasteDetection>()
            .OwnsOne(w => w.Location);
    }

}
