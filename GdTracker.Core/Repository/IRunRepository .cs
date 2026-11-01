using GdTracker.Core.Entities;

namespace GdTracker.Core.Repository;

public interface IRunRepository
{
    Task<Level?> GetLevelByIdAsync(int id, CancellationToken ct);
    Task AddLevelAsync(Level level, CancellationToken ct);
    Task AddRunAsync(Run run, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    void ClearChanges();

}
