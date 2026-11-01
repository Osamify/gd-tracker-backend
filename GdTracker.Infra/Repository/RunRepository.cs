using GdTracker.Core.Entities;
using GdTracker.Core.Repository;
using GdTracker.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace GdTracker.Infra.Repository;

public class RunRepository(TrackerDbContext context) : IRunRepository
{
    public async Task<Level?> GetLevelByIdAsync(int id, CancellationToken ct)
    {
        // Using FindAsync is faster here because it checks memory first before hitting the DB
        return await context.Levels.FindAsync(new object[] { id }, ct);
    }

    public async Task AddLevelAsync(Level level, CancellationToken ct)
    {
        await context.Levels.AddAsync(level, ct);
    }

    public async Task AddRunAsync(Run run, CancellationToken ct)
    {
        await context.Runs.AddAsync(run, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await context.SaveChangesAsync(ct);
    }

    public void ClearChanges()
    {
        // This clears Entity Framework's memory so it forgets the failed insert
        context.ChangeTracker.Clear();
    }

    //LevelProgressStat Work
    public async Task<LevelProgressStat?> GetLevelProgressStatAsync(int levelId, int percentage, CancellationToken ct)
    {
        return await context.LevelProgressStats.FirstOrDefaultAsync(s => s.LevelId == levelId && s.Percentage == percentage, ct);
    }

    public async Task AddProgressStatAsync(LevelProgressStat stat, CancellationToken ct)
    {
        await context.LevelProgressStats.AddAsync(stat, ct);
    }

    public async Task<List<LevelProgressStat>> GetStatsForLevelAsync(int levelId, CancellationToken ct)
    {
        return await context.LevelProgressStats
            .Where(s => s.LevelId == levelId)
            .OrderBy(s => s.Percentage)
            .ToListAsync(ct);
    }


}
