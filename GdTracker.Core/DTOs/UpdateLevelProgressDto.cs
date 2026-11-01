namespace GdTracker.Core.DTOs;

public class UpdateLevelProgressDto
{
    public int? TotalAttempts { get; set; }
    public int? Progress { get; set; }
    public DateTime? DateCompleted { get; set; }
}