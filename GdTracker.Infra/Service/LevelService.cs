using GdTracker.Core.DTOs;
using GdTracker.Core.Entities;
using GdTracker.Core.Repository;
using GdTracker.Core.Service;
using System.Net.Http.Json;

namespace GdTracker.Infra.Service;

public class LevelService(ILevelRepository repository, HttpClient httpClient): ILevelService
{
    public async Task<List<Level>> GetAllLevelsAsync(CancellationToken ct)
    {
        return await repository.GetAllAsync(ct);
    }

    public async Task<object?> GetLevelDetailsAsync(int id, CancellationToken ct)
    {
        var level = await repository.GetByIdAsync(id, ct);
        if (level == null) return null;

        var allRuns = await repository.GetRunsForLevelAndCopiesAsync(id, level.OriginalLevelId, ct);

        return new
        {
            level.Id,
            level.Name,
            level.Creator,
            level.DemonDifficulty,
            level.TotalAttempts,
            level.Progress,
            level.Rating,
            level.TotalSeconds,
            level.LastPlayedAt,
            level.DateCompleted,
            level.GddpTier,
            level.CurrentAredlRank,
            level.Enjoyment,
            level.RatingCount,
            level.ShowcaseVideoId,
            level.NlwTier,
            level.OriginalLevelId,
            level.IsRated,
            Runs = allRuns
        };
    }

    public async Task AddLevelAsync(Level level, CancellationToken ct)
    {
        await repository.AddAsync(level, ct);
    }

    public async Task UpdateLevelAsync(int id, UpdateLevelProgressDto updatedLevel, CancellationToken ct)
    {
        var level = await repository.GetByIdAsync(id, ct);
        if (level == null) return;

        level.TotalAttempts = updatedLevel.TotalAttempts ?? level.TotalAttempts;
        level.Progress = updatedLevel.Progress ?? level.Progress;
        level.DateCompleted = updatedLevel.DateCompleted ?? level.DateCompleted;
        await repository.UpdateAsync(level, ct);
    }

    public async Task<CombinedAttemptsDto?> GetCombinedAttemptsAsync(int id, CancellationToken ct)
    {
        var level = await repository.GetByIdAsync(id, ct);
        if (level == null) return null;

        var totalAttempts = await repository.GetCombinedAttemptsAsync(id, level.OriginalLevelId, ct);

        return new CombinedAttemptsDto
        {
            LevelId = id,
            LevelName = level.Name,
            CombinedAttempts = totalAttempts
        };
    }

    public async Task<int> SyncGddlAsync(CancellationToken ct)
    {
        var levels = await repository.GetLevelsForGddlSyncAsync(ct);
        int updatedCount = 0;

        foreach (var level in levels)
        {
            var gddlUrl = $"https://gdladder.com/api/levels/{level.Id}";
            try
            {
                // HERE IS WHERE YOUR GDDL DTO IS USED!
                var gddlData = await httpClient.GetFromJsonAsync<GddlApiResponse>(gddlUrl, ct);
                if (gddlData != null)
                {
                    level.GddpTier = gddlData.Rating;
                    level.Enjoyment = gddlData.Enjoyment;
                    level.RatingCount = gddlData.RatingCount;
                    level.ShowcaseVideoId = gddlData.Showcase;
                    level.TotalSeconds = (int?)gddlData.Meta?.Seconds;

                    await repository.UpdateAsync(level, ct);
                    updatedCount++;
                }
            }
            catch (HttpRequestException) { /* Ignore 404s */ }
            await Task.Delay(650, ct);
        }
        return updatedCount;
    }

    public async Task<int> SyncAredlAsync(CancellationToken ct)
    {
        var levels = await repository.GetLevelsForAredlSyncAsync(ct);
        int updatedCount = 0;

        foreach (var level in levels)
        {
            var aredlUrl = $"https://api.aredl.net/v2/api/aredl/levels/{level.Id}";
            try
            {
                // HERE IS WHERE YOUR AREDL DTO IS USED!
                var aredlData = await httpClient.GetFromJsonAsync<AredlApiResponse>(aredlUrl, ct);
                if (aredlData != null)
                {
                    level.CurrentAredlRank = aredlData.Position;
                    level.NlwTier = aredlData.NlwTier;

                    await repository.UpdateAsync(level, ct);
                    updatedCount++;
                }
            }
            catch (HttpRequestException) { /* Ignore 404s */ }
            await Task.Delay(650, ct);
        }
        return updatedCount;
    }


}
