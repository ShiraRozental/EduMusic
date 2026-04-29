using Common.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<UserProvisioningDto>> GetUsersByTeacherId(int teacherId);
        Task<AdminDto> GetById(int id);
        Task<AdminDto> UpdateItem(int id, AdminDto adminDto);



    }
}
