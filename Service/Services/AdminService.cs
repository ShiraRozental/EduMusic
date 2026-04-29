using Common.Dto;
using Repository.Interfaces;
using Service.Interfaces;
using Common.Exceptions;
using Repository.Repositories;
using AutoMapper;
using Repository.Entities;
using Common.Exceptions;
using Microsoft.Extensions.Logging;
using static Common.Exceptions.CustomExceptions;

namespace Service.Services
{
    public class AdminService(IUserRepository userRepository, IAdminRepository adminRepository, IMapper mapper, ILogger<AdminService> logger) : IAdminService
    {
        private readonly IAdminRepository _repository = adminRepository;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<AdminService> _logger = logger;
        public async Task<AdminDto> GetById(int id)
        {
            var admin = await _repository.GetById(id);
            if (admin == null)
            {
                _logger.LogWarning("Admin with ID {AdminId} was not found", id);
                throw new NotFoundException($"Admin with ID {id} was not found.");
            }
            return _mapper.Map<AdminDto>(admin);
        }

        public async Task<IEnumerable<UserProvisioningDto>> GetUsersByTeacherId(int teacherId)
        {
            var users = await _userRepository.GetAll(u => u.MyTeacherID == teacherId);
            return _mapper.Map<IEnumerable<UserProvisioningDto>>(users ?? new List<User>());
        }

        public async Task<AdminDto> UpdateItem(int id, AdminDto adminDto)
        {
            var existingAdmin = await _repository.GetById(id);

            if (existingAdmin == null)
                throw new NotFoundException($"Admin with ID {id} not found");
            _mapper.Map(adminDto, existingAdmin);
            existingAdmin.AdminID = id;
            try
            {
                var updated = await _repository.UpdateItem(id, existingAdmin);
                return _mapper.Map<AdminDto>(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update admin {AdminId}", id);
                throw; 
            }
        }
        public async Task DeleteItem(int id) { }
        
    }
}
