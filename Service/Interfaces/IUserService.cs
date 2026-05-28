using Common.Dto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IUserService
    {
        Task ImportUsersFromExcelAsync(IFormFile file, int teacherId);

        Task AddUsersManualAsync(IEnumerable<UserProvisioningDto> dtos, int teacherId);

        Task<IEnumerable<UserDto>> GetAllUsersAsync(int teacherId);
        Task DeleteUserAsync(int userId);
    }
}
