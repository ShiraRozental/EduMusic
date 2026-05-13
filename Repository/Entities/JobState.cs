using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        Classifying,
        Completed,
        Failed
    }

    public class JobState
    {
        [Key] // מגדיר את זה כמפתח ראשי
        public Guid Id { get; set; }

        [Required]
        [EnumDataType(typeof(JobStatus))] // מוודא שהערך שייך ל-Enum
        public JobStatus Status { get; set; } = JobStatus.Queued;

        [MaxLength(1000)] // מגביל את הודעת השגיאה כדי שלא תפוצץ את ה-DB
        public string? ErrorMessage { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        [Required(ErrorMessage = "חובה לציין את שם הקובץ המקורי")]
        [StringLength(255, MinimumLength = 1)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [Url] // מוודא שזה נתיב חוקי (אם זה URL) או פשוט נתיב שאינו ריק
        public string FilePath { get; set; } = string.Empty;

        public string? Lyrics { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }
    }
}
