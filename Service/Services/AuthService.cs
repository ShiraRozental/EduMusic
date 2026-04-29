using AutoMapper;
using Common.Dto;
using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;


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

        if (admin == null || !BCrypt.Net.BCrypt.Verify(adminLogin.Password, admin.Password))
            throw new Exception("Unauthorized");

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

        if (user == null) throw new Exception("User not found");

        var token = _tokenService.GenerateUserToken(user);

        return new AuthResponseDto
        {
            Token = token,
            User = _mapper.Map<UserDto>(user),
            Role = "User",
            FullName = $"User: {user.ID}"
        };
    }

    public async Task<AdminDto> RegisterAdmin(AdminRegisterDto registerDto)
    {
        var admin = _mapper.Map<Admin>(registerDto);
        admin.Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
        var newAdmin = await _adminRepo.AddItem(admin);
        return _mapper.Map<AdminDto>(newAdmin);
    }
}