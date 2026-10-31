using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GdTrackerApi.Data;
using GdTracker.Core.DTOs;
using GdTracker.Core.Entities;
namespace GdTrackerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LevelsController : ControllerBase
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private readonly TrackerDbContext _context;
    private readonly ILogger<LevelsController> _logger;

    public LevelsController(TrackerDbContext context, ILogger<LevelsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    static LevelsController()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "GdTrackerApi/1.0");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    [HttpGet]
    public async Task<IActionResult> GetLevels(CancellationToken ct)
    {
        var levels = await _context.Levels.ToListAsync(ct);
        return Ok(levels);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLevel(int id, CancellationToken ct)
    {
        var ratedLevel = await _context.Levels.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (ratedLevel == null) return NotFound();

        // Get the runs from the level AND its copies
        var allRuns = await _context.Levels
            .Where(l => l.Id == ratedLevel.Id ||
                        l.OriginalLevelId == ratedLevel.Id ||
                        (ratedLevel.OriginalLevelId != null && l.OriginalLevelId == ratedLevel.OriginalLevelId))
            .SelectMany(l => l.Runs)
            .ToListAsync(ct);

        return Ok(new
        {
            ratedLevel.Id,
            ratedLevel.Name,
            ratedLevel.Creator,
            ratedLevel.DemonDifficulty,
            ratedLevel.TotalAttempts,
            ratedLevel.Progress,
            ratedLevel.Rating,
            ratedLevel.TotalSeconds,
            ratedLevel.LastPlayedAt,
            ratedLevel.DateCompleted,
            ratedLevel.GddpTier,
            ratedLevel.CurrentAredlRank,
            ratedLevel.Enjoyment,
            ratedLevel.RatingCount,
            ratedLevel.ShowcaseVideoId,
            ratedLevel.NlwTier,
            ratedLevel.OriginalLevelId,
            ratedLevel.IsRated,
            Runs = allRuns
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddLevel(Level level, CancellationToken ct)
    {
        _context.Levels.Add(level);
        await _context.SaveChangesAsync(ct);
        return Ok(level);
    }

    [HttpPost("batch-sync")]
    public async Task<IActionResult> SyncLevels([FromBody] List<RunPayloadDto> payloads, CancellationToken ct)
    {
        Console.WriteLine($"!!! RECEIVED REQUEST WITH {payloads?.Count ?? 0} LEVELS !!!");

        if (payloads == null || payloads.Count == 0) return BadRequest("No levels provided.");

        var levelIds = payloads.Select(p => p.LevelID).ToList();

        var existingLevels = await _context.Levels
            .Where(l => levelIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, ct);

        foreach (var payload in payloads)
        {
            if (existingLevels.TryGetValue(payload.LevelID, out var existingLevel))
            {
                existingLevel.TotalAttempts = payload.TotalAttempts;
                existingLevel.IsRated = payload.IsRated;
                existingLevel.DemonDifficulty = payload.DemonDifficulty;
                existingLevel.Rating = payload.Rating;

                // ONLY update Name/Creator if the mod actually sent a valid string!
                if (!string.IsNullOrEmpty(payload.LevelName))
                    existingLevel.Name = payload.LevelName;

                if (!string.IsNullOrEmpty(payload.CreatorName))
                    existingLevel.Creator = payload.CreatorName;

                if (existingLevel.OriginalLevelId == null && payload.OriginalLevelID > 0)
                {
                    existingLevel.OriginalLevelId = payload.OriginalLevelID;
                }
            }
            else
            {
                _context.Levels.Add(new Level
                {
                    Id = payload.LevelID,
                    OriginalLevelId = payload.OriginalLevelID > 0 ? payload.OriginalLevelID : null,
                    Name = payload.LevelName,
                    Creator = payload.CreatorName,
                    IsRated = payload.IsRated,
                    DemonDifficulty = payload.DemonDifficulty,
                    Rating = payload.Rating,
                    TotalAttempts = payload.TotalAttempts,
                    Progress = payload.Percentage,
                    IsTracked = true
                });
            }
        }

        await _context.SaveChangesAsync(ct);
        return Ok(new { InsertedOrUpdatedCount = payloads.Count });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLevel(int id, Level updatedLevel, CancellationToken ct)
    {
        var level = await _context.Levels.FindAsync(new object[] { id }, ct);
        if (level == null) return NotFound();

        level.Name = updatedLevel.Name;
        level.Creator = updatedLevel.Creator;
        level.DemonDifficulty = updatedLevel.DemonDifficulty;
        level.GddpTier = updatedLevel.GddpTier;
        level.TotalAttempts = updatedLevel.TotalAttempts;
        level.Progress = updatedLevel.Progress;
        level.DateCompleted = updatedLevel.DateCompleted;

        level.IsRated = updatedLevel.IsRated;
        level.DemonDifficulty = updatedLevel.DemonDifficulty;
        level.Rating = updatedLevel.Rating;

        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("{id}/combined-attempts")]
    public async Task<IActionResult> GetCombinedAttempts(int id, CancellationToken ct)
    {
        var ratedLevel = await _context.Levels.FindAsync(new object[] { id }, ct);
        if (ratedLevel == null)
            return NotFound("Level not found.");

        // Sum attempts for:
        // 1. The rated level itself
        // 2. Copies made directly from this rated level (copy.OriginalLevelId == ratedLevel.Id)
        // 3. Sibling copies sharing the same original (copy.OriginalLevelId == ratedLevel.OriginalLevelId)
        var totalAttempts = await _context.Levels
            .Where(l => l.Id == ratedLevel.Id ||
                        l.OriginalLevelId == ratedLevel.Id ||
                        (ratedLevel.OriginalLevelId != null && l.OriginalLevelId == ratedLevel.OriginalLevelId))
            .SumAsync(l => l.TotalAttempts, ct);

        return Ok(new
        {
            LevelId = id,
            LevelName = ratedLevel.Name,
            CombinedAttempts = totalAttempts
        });
    }

    [HttpPost("sync-gddl-all")]
    public async Task<IActionResult> SyncOldLevels(CancellationToken ct)
    {
        var levels = await _context.Levels.
            Where(l => l.IsRated && l.GddpTier == null)
            .ToListAsync(ct);

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

                    updatedCount++;
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Level {LevelId} not found on GDDL.", level.Id);
            }

            await Task.Delay(650, ct);
        }
        await _context.SaveChangesAsync(ct);
        return Ok(new { message = $"Successfully synced {updatedCount} levels." });
    }

    [HttpPost("sync-aredl-all")]
    public async Task<IActionResult> SyncOldAredlLevels(CancellationToken ct)
    {
        // Filter for Extreme Demons that don't have an AREDL rank synced yet
        var levels = await _context.Levels
            .Where(l => (l.DemonDifficulty == "Extreme" || l.DemonDifficulty == "Extreme Demon") && l.CurrentAredlRank == null)
            .ToListAsync(ct);

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

            // Polite delay to prevent API rate-limiting
            await Task.Delay(650, ct);
        }

        await _context.SaveChangesAsync(ct);
        return Ok(new { message = $"Successfully synced {updatedCount} AREDL levels." });
    }
}