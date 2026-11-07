using GdTracker.Core.DTOs;
using GdTracker.Core.Entities;
using GdTracker.Core.Mappings;
using GdTracker.Core.Repository;
using GdTracker.Core.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Net.Http.Json;

namespace GdTracker.Infra.Service;

public class RunService : IRunService
{
    private readonly IRunRepository _repository;
    private readonly HttpClient _httpClient;
    private readonly ILogger<RunService> _logger;

    public RunService(IRunRepository repository, HttpClient httpClient, ILogger<RunService> logger)
    {
        _repository = repository;
        _httpClient = httpClient;
        _logger = logger;

        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "GdTrackerApi/1.0");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }
    }

    public async Task<int> AddRunAsync(RunPayloadDto payload, CancellationToken ct)
    {
        // 🛑 EARLY RETURN: Ignore the "0% to 100%" glitch on unrated levels
        if (!payload.IsRated && payload.StartPercentage == 0 && payload.Percentage == 100)
        {
            _logger.LogInformation("Skipped broken unrated level {LevelId}: Reported a glitched 0% -> 100% run.", payload.LevelID);
            return payload.LevelID;
        }

        var timestamp = DateTimeOffset.TryParse(payload.Timestamp, out var parsedOffset)
            ? parsedOffset.UtcDateTime
            : DateTime.UtcNow;

        var level = await _repository.GetLevelByIdAsync(payload.LevelID, ct);

        // This prevents replays from ruining your historical completion stats, attempts, or run history.
        if (level != null && level.Progress == 100)
        {
            _logger.LogInformation("Level {LevelId} is already beaten. Ignoring payload to preserve original completion data.", payload.LevelID);
            return payload.LevelID;
        }

        if (level == null)
        {
            int minPercentageToTrack = 10;

            if (payload.DemonDifficulty == "Extreme" || payload.DemonDifficulty == "Insane")
            {
                minPercentageToTrack = 2;
            }
            else if (payload.DemonDifficulty == "Hard")
            {
                minPercentageToTrack = 5;
            }

            if (payload.StartPercentage == 0 && payload.Percentage < minPercentageToTrack)
            {
                _logger.LogInformation("Skipped new level {LevelId}: Run was less than {Threshold}% from 0.", payload.LevelID, minPercentageToTrack);
                return payload.LevelID;
            }
            level = payload.ToLevelEntity(timestamp);

            // Adding Gddl Info For New Levels
            if (level.IsRated)
            {
                var gddlUrl = $"https://gdladder.com/api/levels/{level.Id}";
                try
                {
                    var gddlData = await _httpClient.GetFromJsonAsync<GddlApiResponse>(gddlUrl, ct);
                    if (gddlData != null)
                    {
                        level.GddpTier = gddlData.Rating;
                        level.Enjoyment = gddlData.Enjoyment;
                        level.RatingCount = gddlData.RatingCount;
                        level.ShowcaseVideoId = gddlData.Showcase;
                        level.TotalSeconds = (int?)gddlData.Meta?.Seconds;
                    }
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("Level {LevelId} not found on GDDL (404). Leaving metadata null.", level.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch or parse GDDL data for level {LevelId}.", level.Id);
                }
            }

            // Adding Aredl Info For New Levels
            if (level.DemonDifficulty == "Extreme")
            {
                var aredlUrl = $"https://api.aredl.net/v2/api/aredl/levels/{level.Id}";
                try
                {
                    var aredlData = await _httpClient.GetFromJsonAsync<AredlApiResponse>(aredlUrl, ct);
                    if (aredlData != null)
                    {
                        level.CurrentAredlRank = aredlData.Position;
                        level.NlwTier = aredlData.NlwTier;
                    }
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("Level {LevelId} not found on AREDL (404).", level.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch AREDL data for level {LevelId}.", level.Id);
                }
            }

            await _repository.AddLevelAsync(level, ct);
        }
        else
        {
            if (level.Progress < 100) level.TotalAttempts = payload.TotalAttempts;
            if (level.Name != payload.LevelName) level.Name = payload.LevelName;
            if (level.Creator != payload.CreatorName) level.Creator = payload.CreatorName;
            if (level.OriginalLevelId == null && payload.OriginalLevelID > 0) level.OriginalLevelId = payload.OriginalLevelID;
            if (level.IsRated != payload.IsRated) level.IsRated = payload.IsRated;
            if (level.DemonDifficulty != payload.DemonDifficulty) level.DemonDifficulty = payload.DemonDifficulty;
            if (level.Rating != payload.Rating) level.Rating = payload.Rating;

            level.LastPlayedAt = timestamp;
        }

        if (payload.StartPercentage == 0)
        {
            if (payload.Percentage == 100)
            {
                level.DateCompleted ??= timestamp;
                level.Progress = 100;
                level.AredlRankWhenCompleted ??= level.CurrentAredlRank;
            }
            else if (payload.Percentage > level.Progress)
            {
                level.Progress = payload.Percentage;
            }
        }

        var newRun = payload.ToRunEntity(timestamp);
        await _repository.AddRunAsync(newRun, ct);

        // --- DEATH STATS LOGIC ---
        if (payload.Percentage < 100)
        {
            var stat = await _repository.GetLevelProgressStatAsync(payload.LevelID, payload.Percentage, ct);

            if (stat != null)
            {
                stat.DeathCount++;
                stat.LastDiedAt = timestamp;
            }
            else
            {
                stat = new LevelProgressStat
                {
                    LevelId = payload.LevelID,
                    Percentage = payload.Percentage,
                    DeathCount = 1,
                    LastDiedAt = timestamp
                };
                await _repository.AddProgressStatAsync(stat, ct);
            }
        }

        try
        {
            await _repository.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            _logger.LogInformation("Concurrent insert handled for level {LevelId}. Retrying as update.", payload.LevelID);
            _repository.ClearChanges();

            var existingLevel = await _repository.GetLevelByIdAsync(payload.LevelID, ct);
            if (existingLevel != null)
            {
                existingLevel.LastPlayedAt = timestamp;

                if (existingLevel.Progress < 100) existingLevel.TotalAttempts = payload.TotalAttempts;
                if (payload.Percentage > existingLevel.Progress) existingLevel.Progress = payload.Percentage;

                var retryRun = payload.ToRunEntity(timestamp);
                await _repository.AddRunAsync(retryRun, ct);

                // --- RETRY DEATH STATS LOGIC ---
                if (payload.Percentage < 100)
                {
                    var retryStat = await _repository.GetLevelProgressStatAsync(payload.LevelID, payload.Percentage, ct);

                    if (retryStat != null)
                    {
                        retryStat.DeathCount++;
                        retryStat.LastDiedAt = timestamp;
                    }
                    else
                    {
                        retryStat = new LevelProgressStat
                        {
                            LevelId = payload.LevelID,
                            Percentage = payload.Percentage,
                            DeathCount = 1,
                            LastDiedAt = timestamp
                        };
                        await _repository.AddProgressStatAsync(retryStat, ct);
                    }
                }
                await _repository.SaveChangesAsync(ct);
            }
        }

        _logger.LogInformation("Run saved: level {LevelId} {Start}% -> {End}%", payload.LevelID, payload.StartPercentage, payload.Percentage);

        return payload.LevelID;
    }

    public async Task<List<LevelProgressStat>> GetLevelStatsAsync(int levelId, CancellationToken ct)
    {
        return await _repository.GetStatsForLevelAsync(levelId, ct);
    }
}