using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;


namespace Common.Dto
{
    public class AdminDto
    {
        [Required]
        public int AdminID { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FullName { get; set; }

        public string? ImageUrl { get; set; } 
    }
}
