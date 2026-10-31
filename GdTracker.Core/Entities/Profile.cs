using System.ComponentModel.DataAnnotations;

namespace GdTracker.Core.Entities;

public class Profile
{
    [Key]
    public int Id { get; set; } = 1;

    public string PlayerName { get; set; } = "Osama";
    public string Bio { get; set; } = string.Empty;

    // Global Geometry Dash Stats
    public int GlobalStars { get; set; }
    public int UserCoins { get; set; }
    public int TotalGlobalAttempts { get; set; }

    // Demon Breakdowns
    public int ExtremeDemons { get; set; }
    public int InsaneDemons { get; set; }
    public int HardDemons { get; set; }
    public int MediumDemons { get; set; }
    public int EasyDemons { get; set; }

    // Tracks when you last synced your global stats
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}