
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Repository.Entities
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, MinimumLength = 2)]
        public string CategoryName { get; set; }
        public int? ParentCategoryID { get; set; }

        public int? AdminID { get; set; }
        [ForeignKey("AdminID")]
        public virtual Admin? Admin { get; set; }
    }
}
