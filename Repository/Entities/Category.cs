
using System.ComponentModel.DataAnnotations;


namespace Repository.Entities
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        public string CategoryName { get; set; }

        public virtual ICollection<Song> Songs { get; set; }

    }
}
