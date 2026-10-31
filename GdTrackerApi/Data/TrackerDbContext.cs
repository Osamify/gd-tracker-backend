using Microsoft.EntityFrameworkCore;
using GdTracker.Core.Entities;

namespace GdTrackerApi.Data
{
    public class TrackerDbContext : DbContext
    {
        public TrackerDbContext(DbContextOptions<TrackerDbContext> options) : base(options)
        {
        }

        public DbSet<Level> Levels { get; set; }
        public DbSet<Run> Runs { get; set; }
    }
}
