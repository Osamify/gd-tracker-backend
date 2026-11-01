using Microsoft.EntityFrameworkCore;
using GdTracker.Core.Entities;

namespace GdTracker.Infra.Data;

public class TrackerDbContext : DbContext
{
    public TrackerDbContext(DbContextOptions<TrackerDbContext> options) : base(options)
    {
    }

    public DbSet<Level> Levels { get; set; }
    public DbSet<Run> Runs { get; set; }
    public DbSet<LevelProgressStat> LevelProgressStats { get; set; }
}
