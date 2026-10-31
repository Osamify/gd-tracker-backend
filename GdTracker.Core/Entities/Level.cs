using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GdTracker.Core.Entities;

public class Level
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Creator { get; set; } = string.Empty;

    public string DemonDifficulty { get; set; } = string.Empty;

    public int TotalAttempts { get; set; }

    public int? OriginalLevelId { get; set; }
    public bool IsRated { get; set; }

    // Tracks Epic, Legendary, Mythic, Featured, or None
    public string Rating { get; set; } = string.Empty;

    // Extracted as raw values for proper sorting/filtering
    public int? TotalSeconds { get; set; }

    //Gddl Props
    public double? GddpTier { get; set; }
    public double? Enjoyment { get; set; }
    public int? RatingCount { get; set; }
    public string? ShowcaseVideoId { get; set; }

    //Aredl Props
    public int? CurrentAredlRank { get; set; }
    public int? AredlRankWhenCompleted { get; set; }
    public string? NlwTier { get; set; }

    [NotMapped]
    public int CombinedAttempts { get; set; }

    public int Progress { get; set; }
    public bool IsTracked { get; set; } = true;

    public DateTime? LastPlayedAt { get; set; }
    public DateTime? DateCompleted { get; set; }

    public List<Run> Runs { get; set; } = new();
}