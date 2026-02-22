
using System.ComponentModel.DataAnnotations;

namespace Repository.Entities
{
    public class Admin
    {
        [Key]
        public int AdminID { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string FullName { get; set; }

        public string? ImageUrl { get; set; }

        public virtual ICollection<User> MyUsers { get; set; }

        public virtual ICollection<Song> MySongs { get; set; }
    }
}
