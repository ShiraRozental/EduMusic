using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Common.Dto
{
    public class SongCreateDto
    {
        [Required]
        public IFormFile SongFile { get; set; }

        public string? Title { get; set; }

        public string? Artist { get; set; }
    }
}
