using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dto
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public object User { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }  
    }
}
