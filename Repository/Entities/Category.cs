
using System.ComponentModel.DataAnnotations;


namespace Repository.Entities
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, MinimumLength = 2)]
        public string CategoryName { get; set; }

        public virtual ICollection<Song> Songs { get; set; }

    }
}
