using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entities
{
    public class TagCategory
    {
        [Key]
        public int TagCategoryID { get; set; }

        public int TagID { get; set; }
        [ForeignKey("TagID")]
        public virtual Tag Tag { get; set; }

        public int CategoryID { get; set; }
        [ForeignKey("CategoryID")]
        public virtual Category Category { get; set; }
    }
}
