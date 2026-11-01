using GdTracker.Core.DTOs;

namespace GdTracker.Core.Service;

public interface IRunService
{
    Task<int> AddRunAsync(RunPayloadDto payload, CancellationToken ct);
}
