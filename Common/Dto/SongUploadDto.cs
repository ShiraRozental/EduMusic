using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Common.Dto
{
    public class SongUploadDto
    {
        [Required]
        public IFormFile SongFile { get; set; }

        public string? Title { get; set; }

        public string? Artist { get; set; }

    }
}
