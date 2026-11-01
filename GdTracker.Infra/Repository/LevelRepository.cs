using GdTracker.Core.Entities;
using GdTracker.Core.Repository;
using GdTracker.Infra.Data;
using Microsoft.EntityFrameworkCore;
namespace GdTracker.Infra.Repository;

public class LevelRepository(TrackerDbContext context) : ILevelRepository
{
    public async Task<List<Level>> GetAllAsync(CancellationToken ct)
    {
        return await context.Levels.ToListAsync(ct);
    }

    public async Task<Level?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Levels.FirstOrDefaultAsync(l => l.Id == id, ct);
    }

    public async Task AddAsync(Level level, CancellationToken ct)
    {
        await context.Levels.AddAsync(level, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Level level, CancellationToken ct)
    {
        context.Levels.Update(level);
        await context.SaveChangesAsync(ct);
    }

    public async Task<List<Run>> GetRunsForLevelAndCopiesAsync(int id, int? originalId, CancellationToken ct)
    {
        return await context.Levels
            .Where(l => l.Id == id ||
                        l.OriginalLevelId == id ||
                        (originalId != null && l.OriginalLevelId == originalId))
            .SelectMany(l => l.Runs)
            .ToListAsync(ct);
    }

    public async Task<int> GetCombinedAttemptsAsync(int id, int? originalId, CancellationToken ct)
    {
        return await context.Levels
            .Where(l => l.Id == id ||
                        l.OriginalLevelId == id ||
                        (originalId != null && l.OriginalLevelId == originalId))
            .SumAsync(l => l.TotalAttempts, ct);
    }

    public async Task<List<Level>> GetLevelsForGddlSyncAsync(CancellationToken ct)
    {
        return await context.Levels
            .Where(l => l.IsRated)
            .ToListAsync(ct);
    }

    public async Task<List<Level>> GetLevelsForAredlSyncAsync(CancellationToken ct)
    {
        return await context.Levels
            .Where(l => l.DemonDifficulty == "Extreme")
            .ToListAsync(ct);
    }







}
