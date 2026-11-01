using GdTracker.Core.DTOs;
using GdTracker.Core.Mappings;
using GdTracker.Core.Entities;
using GdTracker.Core.Repository;
using GdTracker.Core.Service;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace GdTracker.Infra.Service;

public class LevelService : ILevelService
{
    private readonly ILevelRepository _repository;
    private readonly HttpClient _httpClient;
    private readonly ILogger<LevelService> _logger;

    public LevelService(ILevelRepository repository, HttpClient httpClient, ILogger<LevelService> logger)
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

    public async Task<List<Level>> GetAllLevelsAsync(CancellationToken ct)
    {
        return await _repository.GetAllAsync(ct);
    }

    public async Task<LevelDetailsDto?> GetLevelDetailsAsync(int id, CancellationToken ct)
    {
        var level = await _repository.GetByIdAsync(id, ct);
        if (level == null) return null;

        var allRuns = await _repository.GetRunsForLevelAndCopiesAsync(id, level.OriginalLevelId, ct);

        return level.ToDto(allRuns);
    }

    public async Task AddLevelAsync(Level level, CancellationToken ct)
    {
        await _repository.AddAsync(level, ct);
    }

    public async Task UpdateLevelAsync(int id, UpdateLevelProgressDto updatedLevel, CancellationToken ct)
    {
        var level = await _repository.GetByIdAsync(id, ct);
        if (level == null) return;

        level.TotalAttempts = updatedLevel.TotalAttempts ?? level.TotalAttempts;
        level.Progress = updatedLevel.Progress ?? level.Progress;
        level.DateCompleted = updatedLevel.DateCompleted ?? level.DateCompleted;

        await _repository.UpdateAsync(level, ct);
    }

    public async Task<CombinedAttemptsDto?> GetCombinedAttemptsAsync(int id, CancellationToken ct)
    {
        var level = await _repository.GetByIdAsync(id, ct);
        if (level == null) return null;

        var totalAttempts = await _repository.GetCombinedAttemptsAsync(id, level.OriginalLevelId, ct);

        return new CombinedAttemptsDto
        {
            LevelId = id,
            LevelName = level.Name,
            CombinedAttempts = totalAttempts
        };
    }

    public async Task<int> SyncGddlAsync(CancellationToken ct)
    {
        var levels = await _repository.GetLevelsForGddlSyncAsync(ct);
        int updatedCount = 0;

        foreach (var level in levels)
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

                    await _repository.UpdateAsync(level, ct);
                    updatedCount++;
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Level {LevelId} not found on GDDL.", level.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch GDDL data for level {LevelId}.", level.Id);
            }
            await Task.Delay(650, ct);
        }
        return updatedCount;
    }

    public async Task<int> SyncAredlAsync(CancellationToken ct)
    {
        var levels = await _repository.GetLevelsForAredlSyncAsync(ct);
        int updatedCount = 0;

        foreach (var level in levels)
        {
            var aredlUrl = $"https://api.aredl.net/v2/api/aredl/levels/{level.Id}";
            try
            {
                var aredlData = await _httpClient.GetFromJsonAsync<AredlApiResponse>(aredlUrl, ct);
                if (aredlData != null)
                {
                    level.CurrentAredlRank = aredlData.Position;
                    level.NlwTier = aredlData.NlwTier;

                    await _repository.UpdateAsync(level, ct);
                    updatedCount++;
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Level {LevelId} not found on AREDL.", level.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch AREDL data for level {LevelId}.", level.Id);
            }
            await Task.Delay(650, ct);
        }
        return updatedCount;
    }
}