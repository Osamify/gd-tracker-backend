using GdTracker.Core.DTOs;
using GdTracker.Core.Entities;

namespace GdTracker.Core.Mappings;

public static class RunMappings
{
    // Maps RunPayloadDto to a NEW Run Entity
    public static Run ToRunEntity(this RunPayloadDto dto, DateTime timestamp)
    {
        return new Run
        {
            LevelId = dto.LevelID,
            StartPercent = dto.StartPercentage,
            EndPercent = dto.Percentage,
            TotalAttempts = dto.TotalAttempts,
            RunTime = dto.RunTime,
            Timestamp = timestamp
        };
    }

    // Maps RunPayloadDto to a NEW Level Entity (when the level doesn't exist)
    public static Level ToLevelEntity(this RunPayloadDto dto, DateTime timestamp)
    {
        return new Level
        {
            Id = dto.LevelID,
            OriginalLevelId = dto.OriginalLevelID > 0 ? dto.OriginalLevelID : null,
            Name = dto.LevelName,
            Creator = dto.CreatorName,
            DemonDifficulty = dto.DemonDifficulty != "None" ? dto.DemonDifficulty : "Urrated",
            Rating = dto.Rating,
            TotalAttempts = dto.TotalAttempts,
            IsRated = dto.IsRated,
            IsTracked = false,
            LastPlayedAt = timestamp
        };
    }

}
