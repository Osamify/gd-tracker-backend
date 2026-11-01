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
        var timestamp = DateTimeOffset.TryParse(payload.Timestamp, out var parsedOffset)
            ? parsedOffset.UtcDateTime
            : DateTime.UtcNow;

        var level = await _repository.GetLevelByIdAsync(payload.LevelID, ct);

        if (level == null)
        {
            _logger.LogInformation("DEBUG: Level {LevelId} not found. Creating new level.", payload.LevelID);
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
            _logger.LogInformation("DEBUG: Level {LevelId} exists. Updating existing level.", payload.LevelID);
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
        _logger.LogInformation("DEBUG: Run staged in memory for Level {LevelId}", payload.LevelID);

        // --- DEATH STATS LOGIC ---
        _logger.LogInformation("DEBUG: Checking Death Stat for Level {LevelId} at {Percentage}%", payload.LevelID, payload.Percentage);

        if (payload.Percentage < 100)
        {
            var stat = await _repository.GetLevelProgressStatAsync(payload.LevelID, payload.Percentage, ct);

            if (stat != null)
            {
                _logger.LogInformation("DEBUG: Found existing stat! Incrementing DeathCount to {Count}", stat.DeathCount + 1);
                stat.DeathCount++;
                stat.LastDiedAt = timestamp;
            }
            else
            {
                _logger.LogInformation("DEBUG: Stat not found. Staging new LevelProgressStat.");
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
        else
        {
            _logger.LogInformation("DEBUG: Skipping Death Stat because Percentage is 100.");
        }

        _logger.LogInformation("DEBUG: Calling SaveChangesAsync...");
        try
        {
            await _repository.SaveChangesAsync(ct);
            _logger.LogInformation("DEBUG: SaveChangesAsync SUCCESS!");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            _logger.LogInformation("DEBUG: Concurrency error 23505 caught! Retrying as update.");
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
                _logger.LogInformation("DEBUG: Retry SaveChangesAsync SUCCESS!");
            }
            else
            {
                _logger.LogWarning("DEBUG: Concurrency error, but could not find existing level to update!");
            }
        }

        _logger.LogInformation("Run saved: level {LevelId} {Start}% -> {End}%", payload.LevelID, payload.StartPercentage, payload.Percentage);

        return payload.LevelID;
    }
}