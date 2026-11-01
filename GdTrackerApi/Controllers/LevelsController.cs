using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GdTracker.Infra.Data;
using GdTracker.Core.DTOs;
using GdTracker.Core.Entities;
using GdTracker.Core.Service;
namespace GdTrackerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LevelsController(ILevelService levelService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLevels(CancellationToken ct)
    {
        var levels = await levelService.GetAllLevelsAsync(ct);
        return Ok(levels);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLevel(int id, CancellationToken ct)
    {
        var level = await levelService.GetLevelDetailsAsync(id, ct);
        if (level == null) return NotFound();
        return Ok(level);
    }

    [HttpPost]
    public async Task<IActionResult> AddLevel(Level level, CancellationToken ct)
    {
        await levelService.AddLevelAsync(level, ct);
        return Ok(level);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLevel(int id, [FromBody] UpdateLevelProgressDto updatedLevel, CancellationToken ct)
    {
        await levelService.UpdateLevelAsync(id, updatedLevel, ct);
        return NoContent();
    }

    [HttpGet("{id}/combined-attempts")]
    public async Task<IActionResult> GetCombinedAttempts(int id, CancellationToken ct)
    {
        var result = await levelService.GetCombinedAttemptsAsync(id, ct);
        if (result == null) return NotFound("Level not found.");
        return Ok(result);
    }

    [HttpPost("sync-gddl-all")]
    public async Task<IActionResult> SyncOldLevels(CancellationToken ct)
    {
        var count = await levelService.SyncGddlAsync(ct);
        return Ok(new { message = $"successfully synced {count} levels." });
    }

    [HttpPost("sync-aredl-all")]
    public async Task<IActionResult> SyncOldAredlLevels(CancellationToken ct)
    {
        var count = await levelService.SyncAredlAsync(ct);
        return Ok(new { message = $"Successfully synced {count} AREDL levels." });
    }
}