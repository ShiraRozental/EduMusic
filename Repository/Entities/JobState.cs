using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entities
{
    public enum JobStatus
    {
        Queued,
        SeparatingVocals,
        Transcribing,
        FixingLyrics,
        NormalizingWords,
        SynchronizingTags,
        Classifying,
        Completed,
        Failed
    }

    public class JobState
    {
        [Key] 
        public Guid Id { get; set; }

        [Required]
        [EnumDataType(typeof(JobStatus))] 
        public JobStatus Status { get; set; } = JobStatus.Queued;

        [MaxLength(1000)] 
        public string? ErrorMessage { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public int SongID { get; set; }
        [ForeignKey("SongID")]
        public virtual Song Song { get; set; }
    }
}
