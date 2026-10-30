using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GdTrackerApi.Models
{
    public class Run
    {
        [Key]
        public int Id { get; set; }

        public int LevelId { get; set; }

        [ForeignKey("LevelId")]
        public Level? Level { get; set; }

        public int StartPercent { get; set; }
        public int EndPercent { get; set; }

        // Attempts count at the exact moment of this run
        public int TotalAttempts { get; set; }

        // How long the run/attempt lasted in seconds
        public float RunTime { get; set; }

        // When the death or completion occurred
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}