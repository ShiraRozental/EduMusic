
using System.ComponentModel.DataAnnotations;

namespace Repository.Entities
{
    public class Admin
    {
        [Key]
        public int AdminID { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string FullName { get; set; }

        public string ImageUrl { get; set; }

        public virtual ICollection<User> MyUsers { get; set; }

        public virtual ICollection<Song> MySongs { get; set; }
    }
}
