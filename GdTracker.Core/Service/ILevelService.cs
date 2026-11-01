using GdTracker.Core.DTOs;
using GdTracker.Core.Entities;

namespace GdTracker.Core.Service;

public interface ILevelService
{
    Task<List<Level>> GetAllLevelsAsync(CancellationToken ct);
    Task<LevelDetailsDto?> GetLevelDetailsAsync(int id, CancellationToken ct);
    Task AddLevelAsync(Level level, CancellationToken ct);
    Task UpdateLevelAsync(int id, UpdateLevelProgressDto updatedLevel, CancellationToken ct);
    Task<CombinedAttemptsDto?> GetCombinedAttemptsAsync(int id, CancellationToken ct);

    // Sync Methods
    Task<int> SyncGddlAsync(CancellationToken ct);
    Task<int> SyncAredlAsync(CancellationToken ct);
}