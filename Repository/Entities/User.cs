using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Repository.Entities
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "Identity number must be exactly 9 digits")]
        public string ID { get; set; }

        public int MyTeacherID { get; set; }

        [ForeignKey("MyTeacherID")]
        public virtual Admin MyTeacher { get; set; }
    }
}
