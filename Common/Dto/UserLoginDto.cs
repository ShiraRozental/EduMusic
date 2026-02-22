using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dto
{
    public class UserLoginDto
    {

        [Required(ErrorMessage = "Identity Number is required")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "Identity Number must be exactly 9 digits")]
        public string ID { get; set; }

    }
}
