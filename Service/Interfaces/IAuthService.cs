using Common.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAdmin(AdminRegisterDto registerDto);
        Task<AuthResponseDto> LoginAdmin(AdminLoginDto adminLogin);
        Task<AuthResponseDto> LoginUser(UserLoginDto userLogin);
    }
}
