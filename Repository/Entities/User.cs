using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Repository.Entities
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        public string ID { get; set; }

        public int MyTeacherID { get; set; }

        [ForeignKey("MyTeacherID")]
        public virtual Admin MyTeacher { get; set; }
    }
}
