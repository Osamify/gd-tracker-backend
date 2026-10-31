using System.ComponentModel;
using System.Text.Json;
using GdTrackerApi.Data;
using GdTracker.Core.DTOs;
using GdTracker.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace GdTrackerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RunsController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly HttpClient _httpClient = new HttpClient();


    private readonly TrackerDbContext _context;
    private readonly ILogger<RunsController> _logger;

    public RunsController(TrackerDbContext context, ILogger<RunsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> AddRun(CancellationToken ct)
    {
        RunPayloadDto? payload;
        try
        {
            payload = await JsonSerializer.DeserializeAsync<RunPayloadDto>(Request.Body, JsonOptions, ct);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid JSON received on /api/runs");
            return BadRequest($"Invalid JSON: {ex.Message}");
        }

        if (payload == null)
            return BadRequest("Invalid payload.");

        var timestamp = DateTimeOffset.TryParse(payload.Timestamp, out var parsedOffset)
            ? parsedOffset.UtcDateTime
            : DateTime.UtcNow;

        var level = await _context.Levels.FindAsync(new object[] { payload.LevelID }, ct);
        if (level == null)
        {
            level = new Level
            {
                Id = payload.LevelID,
                OriginalLevelId = payload.OriginalLevelID > 0 ? payload.OriginalLevelID : null,
                Name = payload.LevelName,
                Creator = payload.CreatorName,
                DemonDifficulty = payload.DemonDifficulty != "None" ? payload.DemonDifficulty : "Urrated",
                Rating = payload.Rating,
                TotalAttempts = payload.TotalAttempts,
                IsRated = payload.IsRated,
                IsTracked = false,
                LastPlayedAt = timestamp
            };


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
                    // Normal behavior: The level is rated but is not a demon tracked by GDDL.
                    _logger.LogInformation("Level {LevelId} not found on GDDL (404). Leaving metadata null.", level.Id);
                }
                catch (Exception ex)
                {
                    // Abnormal behavior: GDDL is down, timeout, or the JSON structure changed.
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
                    // The level is an extreme demon, but not on the AREDL yet (e.g., newly rated or unplaced)
                    _logger.LogInformation("Level {LevelId} not found on AREDL (404).", level.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch AREDL data for level {LevelId}.", level.Id);
                }
            }

            _context.Levels.Add(level);
        }
        else
        {
            if (level.Progress < 100)
            {
                level.TotalAttempts = payload.TotalAttempts;
            }

            if (level.Name != payload.LevelName)
            {
                level.Name = payload.LevelName;
            }

            if (level.Creator != payload.CreatorName)
            {
                level.Creator = payload.CreatorName;
            }

            if (level.OriginalLevelId == null && payload.OriginalLevelID > 0)
            {
                level.OriginalLevelId = payload.OriginalLevelID;
            }

            if (level.IsRated != payload.IsRated)
            {
                level.IsRated = payload.IsRated;
            }

            if (level.DemonDifficulty != payload.DemonDifficulty)
            {
                level.DemonDifficulty = payload.DemonDifficulty;
            }

            if (level.Rating != payload.Rating)
            {
                level.Rating = payload.Rating;
            }

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

        var newRun = new Run
        {
            LevelId = payload.LevelID,
            StartPercent = payload.StartPercentage,
            EndPercent = payload.Percentage,
            TotalAttempts = payload.TotalAttempts,
            RunTime = payload.RunTime,
            Timestamp = timestamp
        };

        _context.Runs.Add(newRun);

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            // Concurrency catch: Another request just created this level ID a millisecond ago
            _logger.LogInformation("Concurrent insert handled for level {LevelId}. Retrying as update.", payload.LevelID);

            // Clear the failed insert attempt from Entity Framework's memory
            _context.ChangeTracker.Clear();

            // Fetch the level that the OTHER concurrent request just successfully saved
            var existingLevel = await _context.Levels.FindAsync(new object[] { payload.LevelID }, ct);
            if (existingLevel != null)
            {
                existingLevel.LastPlayedAt = timestamp;

                if (existingLevel.Progress < 100)
                {
                    existingLevel.TotalAttempts = payload.TotalAttempts;
                }

                if (payload.Percentage > existingLevel.Progress)
                {
                    existingLevel.Progress = payload.Percentage;
                }

                var retryRun = new Run
                {
                    LevelId = payload.LevelID,
                    StartPercent = payload.StartPercentage,
                    EndPercent = payload.Percentage,
                    TotalAttempts = payload.TotalAttempts,
                    RunTime = payload.RunTime,
                    Timestamp = timestamp
                };

                _context.Runs.Add(retryRun);
                await _context.SaveChangesAsync(ct);
            }
        }

        _logger.LogInformation("Run saved: level {LevelId} {Start}% -> {End}%",
            payload.LevelID, payload.StartPercentage, payload.Percentage);

        return Ok(new { message = "Run logged successfully", levelId = payload.LevelID });
    }
}