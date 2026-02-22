using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dto
{
    public class SongDto
    {
        [Required]
        public int SongID { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Artist { get; set; }

        [Required]
        public int Duration { get; set; }

        [Required]
        public string FilePath { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [Required]
        public int UploaderID { get; set; }

    }
}
