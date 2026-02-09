using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entities
{
    public class Tag
    {
        [Key]
        public int TagID { get; set; }

        [Required]
        public string TagText { get; set; }

        public virtual ICollection<SongTagFrequency> TagsFrequencies { get; set; }
    }
}
