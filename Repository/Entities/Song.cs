using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entities
{
    public class Song
    {
        [Key]
        public int SongID { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [StringLength(100)]
        public string Artist { get; set; }

        [Required]
        public string FilePath { get; set; }
        
        [Required]
        public string RawLyrics { get; set; }

        public int Duration { get; set; }

        public int CategoryID { get; set; }
        [ForeignKey("CategoryID")]
        public virtual Category Category { get; set; }

        public int UploaderID { get; set; }
        [ForeignKey("UploaderID")]
        public virtual Admin Uploader { get; set; }


        public virtual ICollection<SongTagFrequency> TagsFrequencies { get; set; }

    }
}
