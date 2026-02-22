using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dto
{
    public class AdminRegisterDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        [RegularExpression(@"^(?=.*[0-9])(?=.*[a-zA-Z]).{6,20}$",
                    ErrorMessage = "Password must contain both letters and numbers")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(50, MinimumLength = 2)]
        public string FullName { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}
