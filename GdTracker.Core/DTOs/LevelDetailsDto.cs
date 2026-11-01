using GdTracker.Core.Entities;

namespace GdTracker.Core.DTOs;

public class LevelDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Creator { get; set; } = string.Empty;
    public string DemonDifficulty { get; set; } = string.Empty;
    public int TotalAttempts { get; set; }
    public int Progress { get; set; }
    public string? Rating { get; set; }
    public int? TotalSeconds { get; set; }
    public DateTime? LastPlayedAt { get; set; }
    public DateTime? DateCompleted { get; set; }
    public double? GddpTier { get; set; }
    public int? CurrentAredlRank { get; set; }
    public double? Enjoyment { get; set; }
    public int? RatingCount { get; set; }
    public string? ShowcaseVideoId { get; set; }
    public string? NlwTier { get; set; }
    public int? OriginalLevelId { get; set; }
    public bool IsRated { get; set; }

    public List<Run> Runs { get; set; } = new();
}