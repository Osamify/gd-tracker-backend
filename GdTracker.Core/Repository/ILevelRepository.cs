using GdTracker.Core.Entities;

namespace GdTracker.Core.Repository;

public interface ILevelRepository
{
    Task<List<Level>> GetAllAsync(CancellationToken ct);
    Task<Level?> GetByIdAsync(int id, CancellationToken ct);
    Task AddAsync(Level level, CancellationToken ct);
    Task UpdateAsync(Level level, CancellationToken ct);
    Task<List<Run>> GetRunsForLevelAndCopiesAsync(int id, int? originalId, CancellationToken ct);
    Task<int> GetCombinedAttemptsAsync(int id, int? originalId, CancellationToken ct);
    Task<List<Level>> GetLevelsForGddlSyncAsync(CancellationToken ct);
    Task<List<Level>> GetLevelsForAredlSyncAsync(CancellationToken ct);
}