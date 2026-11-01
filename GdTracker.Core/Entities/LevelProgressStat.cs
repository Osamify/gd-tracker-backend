using System.ComponentModel.DataAnnotations;

namespace GdTracker.Core.Entities;

public class LevelProgressStat
{
    [Key]
    public int Id { get; set; }
    public int LevelId { get; set; }
    public int Percentage { get; set; }
    public int DeathCount { get; set; }
    public DateTime LastDiedAt { get; set; }

}
