using GdTracker.Core.DTOs;
using GdTracker.Core.Service;
using Microsoft.AspNetCore.Mvc;

namespace GdTrackerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RunsController : ControllerBase
{
    private readonly IRunService _runService;
    private readonly ILogger<RunsController> _logger;

    public RunsController(IRunService runService, ILogger<RunsController> logger)
    {
        _runService = runService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> AddRun([FromBody] RunPayloadDto payload, CancellationToken ct)
    {
        if (payload == null)
        {
            _logger.LogWarning("Invalid payload received on /api/runs");
            return BadRequest("Invalid payload.");
        }

        var levelId = await _runService.AddRunAsync(payload, ct);

        return Ok(new { message = "Run logged successfully", levelId = levelId });
    }

    [HttpGet("stats/{levelId}")]
    public async Task<IActionResult> GetLevelStats(int levelId, CancellationToken ct)
    {
        var stats = await _runService.GetLevelStatsAsync(levelId, ct);
        return Ok(stats);
    }
}