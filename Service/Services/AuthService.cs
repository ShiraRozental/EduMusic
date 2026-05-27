using AutoMapper;
using Common.Dto;
using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using static Common.Exceptions.CustomExceptions;

public class AuthService(IUserRepository userRepo, IAdminRepository adminRepo, IMapper mapper, ITokenService tokenService) : IAuthService
{
    private readonly IUserRepository _userRepo = userRepo;
    private readonly IAdminRepository _adminRepo = adminRepo;
    private readonly IMapper _mapper = mapper;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<AuthResponseDto> LoginAdmin(AdminLoginDto adminLogin)
    {
        var admins = await _adminRepo.GetAll();
        var admin = admins.FirstOrDefault(a => a.Email == adminLogin.Email);

        // Throws 401 Unauthorized via middleware if admin is not found or password verification fails
        if (admin == null || !BCrypt.Net.BCrypt.Verify(adminLogin.Password, admin.Password))
            throw new UnauthorizedException("Invalid email or password.");

        var token = _tokenService.GenerateAdminToken(admin);

        return new AuthResponseDto
        {
            Token = token,
            User = _mapper.Map<AdminDto>(admin),
            Role = "Admin",
            FullName = admin.FullName
        };
    }

    public async Task<AuthResponseDto> LoginUser(UserLoginDto userLogin)
    {
        var users = await _userRepo.GetAll();
        var user = users.FirstOrDefault(u => u.ID == userLogin.ID);

        // Throws 404 Not Found via middleware if the user does not exist
        if (user == null)
            throw new NotFoundException($"User with ID {userLogin.ID} was not found.");

        var token = _tokenService.GenerateUserToken(user);

        return new AuthResponseDto
        {
            Token = token,
            User = _mapper.Map<UserDto>(user),
            Role = "User",
            FullName = $"User: {user.ID}"
        };
    }

    public async Task<AuthResponseDto> RegisterAdmin(AdminRegisterDto registerDto)
    {
        var existingByEmail = await _adminRepo.GetAdminByEmail(registerDto.Email);

        if (existingByEmail != null)
            throw new ConflictException("An administrator with this email already exists.");

        var admin = _mapper.Map<Admin>(registerDto);
        admin.Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
        
        //409
        var newAdmin = await _adminRepo.AddItem(admin);

        return new AuthResponseDto
        {
            Token = _tokenService.GenerateAdminToken(newAdmin),
            User = _mapper.Map<AdminDto>(newAdmin),
            Role = "Admin",
            FullName = newAdmin.FullName
        };
    }
}