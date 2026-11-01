using GdTracker.Core.Entities;
using GdTracker.Core.Repository;
using GdTracker.Infra.Data;

namespace GdTracker.Infra.Repository;

public class RunRepository(TrackerDbContext context) : IRunRepository
{
    public async Task<Level?> GetLevelByIdAsync(int id, CancellationToken ct)
    {
        // Using FindAsync is faster here because it checks memory first before hitting the DB
        return await context.Levels.FindAsync(id, ct);
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

    public async void ClearChanges()
    {
        // This clears Entity Framework's memory so it forgets the failed insert
        context.ChangeTracker.Clear();
    }


}
