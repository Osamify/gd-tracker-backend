using System.Text.Json.Serialization;

namespace GdTracker.Core.DTOs
{
    public class RunPayloadDto
    {
        public int StartPercentage { get; set; }
        public int Percentage { get; set; }
        public int LevelID { get; set; }
        public int OriginalLevelID { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string CreatorName { get; set; } = string.Empty;
        public string DemonDifficulty { get; set; } = string.Empty;
        public bool IsRated { get; set; }
        public string Rating { get; set; } = string.Empty;
        public float RunTime { get; set; }
        public int TotalAttempts { get; set; }
        public string Timestamp { get; set; } = string.Empty;
    }
}