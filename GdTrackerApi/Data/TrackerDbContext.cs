using Microsoft.EntityFrameworkCore;
using GdTrackerApi.Models;

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
