using GdTracker.Core.DTOs;
using GdTracker.Core.Entities;

namespace GdTracker.Core.Service;

public interface IRunService
{
    Task<int> AddRunAsync(RunPayloadDto payload, CancellationToken ct);

    Task<List<LevelProgressStat>> GetLevelStatsAsync(int levelId, CancellationToken ct);
}
