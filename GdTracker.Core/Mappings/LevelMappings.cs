using GdTracker.Core.Entities;
using GdTracker.Core.DTOs;

namespace GdTracker.Core.Mappings;

public static class LevelMappings
{
    public static LevelDetailsDto ToDto(this Level level, List<Run> runs)
    {
        return new LevelDetailsDto
        {
            Id = level.Id,
            Name = level.Name,
            Creator = level.Creator,
            DemonDifficulty = level.DemonDifficulty,
            TotalAttempts = level.TotalAttempts,
            Progress = level.Progress,
            Rating = level.Rating,
            TotalSeconds = level.TotalSeconds,
            LastPlayedAt = level.LastPlayedAt,
            DateCompleted = level.DateCompleted,
            GddpTier = level.GddpTier,
            CurrentAredlRank = level.CurrentAredlRank,
            Enjoyment = level.Enjoyment,
            RatingCount = level.RatingCount,
            ShowcaseVideoId = level.ShowcaseVideoId,
            NlwTier = level.NlwTier,
            OriginalLevelId = level.OriginalLevelId,
            IsRated = level.IsRated,
            Runs = runs
        };
    }
}
