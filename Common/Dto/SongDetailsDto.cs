using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dto
{
    public class SongDetailsDto
    {
        [Required]
        public string Title { get; set; }
        
        [Required]
        public string Artist { get; set; }

        [Required]
        public string RawLyrics { get; set; }

        [Required]
        public int Duration { get; set; }

        [Required]
        public string CategoryName { get; set; }

        [Required]
        public string UploaderName { get; set; }

        [Required]
        public string FilePath { get; set; }
    }
}
