using Common.Dto;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Repository.Entities;
using Repository.Interfaces;
using Repository.Repositories;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common.Exceptions.CustomExceptions;

namespace Service.Services
{
    public class UserService(IUserRepository userRepository, IAdminRepository adminRepository) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        public readonly IAdminRepository _adminRepository = adminRepository;

        public async Task AddUsersManualAsync(IEnumerable<UserProvisioningDto> dtos, int teacherId)
        {
            foreach (var dto in dtos)
            {
                var existing = await _userRepository.GetUserByIdentityCard(dto.ID);
                if (existing != null)
                {
                    throw new ConflictException($"תלמיד עם ת.ז {dto.ID} כבר רשום במערכת אצל מורה אחר. פנה למורה להסיר אותו תחילה.");
                }
            }

            var newUsers = dtos.Select(dto => new User
            {
                ID = dto.ID,
                FullNameUser = dto.FullNameUser,
                MyTeacherID = teacherId,
            }).ToList();

            if (newUsers.Any())
                await _userRepository.AddUsersRangeAsync(newUsers);
        }

        public async Task ImportUsersFromExcelAsync(IFormFile file, int teacherId)
        {
            var usersToSave = new List<User>();
            var duplicates = new List<string>();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var stream = file.OpenReadStream())
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    reader.Read();
                    while (reader.Read())
                    {
                        var id = reader.GetValue(1)?.ToString();
                        var name = reader.GetValue(0)?.ToString();

                        if (string.IsNullOrEmpty(id)) continue;

                        var existing = await _userRepository.GetUserByIdentityCard(id);
                        if (existing != null)
                        {
                            duplicates.Add($"{name} ({id})");
                            continue;
                        }

                        usersToSave.Add(new User
                        {
                            ID = id,
                            FullNameUser = name,
                            MyTeacherID = teacherId,
                        });
                    }
                }
            }

            if (usersToSave.Any())
                await _userRepository.AddUsersRangeAsync(usersToSave);

            if (duplicates.Any())
                throw new ConflictException( $"התלמידים הבאים כבר קיימים במערכת: {string.Join(", ", duplicates)}");
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(int teacherId)
        {
            var admin = await _adminRepository.GetById(teacherId);
            var users = await _userRepository.GetAll(u =>
                u.MyTeacherID == teacherId && u.ID != admin.ID);

            return users.Select(u => new UserDto
            {
                UserID = u.UserID,
                ID = u.ID,
                FullName = u.FullNameUser,
                MyTeacherID = u.MyTeacherID
            });
        }

        public async Task DeleteUserAsync(int userId)
        {
            await _userRepository.DeleteItem(userId);
        }
    }
}
