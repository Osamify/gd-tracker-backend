using GdTracker.Core.DTOs;
using GdTracker.Core.Entities;
using GdTracker.Core.Service;
using GdTracker.Infra.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.ComponentModel;
using System.Text.Json;
namespace GdTrackerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RunsController : ControllerBase
{
    private readonly IRunService _runService;
    private readonly ILogger<RunsController> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public RunsController(IRunService runService, ILogger<RunsController> logger)
    {
        _runService = runService;
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

        var levelId = await _runService.AddRunAsync(payload, ct);

        return Ok(new { message = "Run logged successfully", levelId = levelId });
    }
}