using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dto
{
    public class UserDto
    {
        [Required]
        public int UserID { get; set; }

        [Required]
        public string ID { get; set; }

        [Required]
        public int MyTeacherID { get; set; }

    }

}
