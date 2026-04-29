using Common.Dto;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class UserService(IUserRepository userRepository) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        public async Task ImportUsersFromExcelAsync(IFormFile file, int teacherId)
        {
            var usersToSave = new List<User>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using (var stream = file.OpenReadStream())
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    reader.Read();

                    while (reader.Read())
                    {
                        var id = reader.GetValue(0)?.ToString();
                        var name = reader.GetValue(1)?.ToString();

                        if (!string.IsNullOrEmpty(id))
                        {
                            usersToSave.Add(new User
                            {
                                ID = id,
                                FullNameUser = name,
                                MyTeacherID = teacherId,
                            });
                        }
                    }
                }
            }

            if (usersToSave.Any())
            {
                await _userRepository.AddUsersRangeAsync(usersToSave);
            }
        }

        public async Task AddUsersManualAsync(IEnumerable<UserProvisioningDto> dtos, int teacherId)
        {
            var newUsers = dtos.Select(dto => new User
            {
                ID = dto.ID,
                FullNameUser = dto.FullNameUser,
                MyTeacherID = teacherId,
            }).ToList();

            if (newUsers.Any())
            {
                await _userRepository.AddUsersRangeAsync(newUsers);
            }
        }
    }
}
