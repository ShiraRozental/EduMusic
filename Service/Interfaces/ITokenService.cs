using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface ITokenService
    {
        string GenerateAdminToken(Admin admin);
        string GenerateUserToken(User user);
    }
}

