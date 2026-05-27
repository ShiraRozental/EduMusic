using Common.Dto;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using System.Threading.Tasks;

namespace EduMusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        // POST: api/auth/register-admin
        [HttpPost("register-admin")]
        public async Task<ActionResult<AuthResponseDto>> RegisterAdmin([FromBody] AdminRegisterDto registerDto)
        {
            // Business exceptions (e.g., ConflictException if email exists) are handled globally by ExceptionMiddleware.
            AuthResponseDto response = await _authService.RegisterAdmin(registerDto);
            return Ok(response);
        }

        // POST: api/auth/login-admin
        [HttpPost("login-admin")]
        public async Task<ActionResult<AuthResponseDto>> LoginAdmin([FromBody] AdminLoginDto loginDto)
        {
            // If authentication fails, the service throws UnauthorizedException, which the middleware maps to a 401 response.
            var response = await _authService.LoginAdmin(loginDto);
            return Ok(response);
        }

        // POST: api/auth/login-user
        [HttpPost("login-user")]
        public async Task<ActionResult<AuthResponseDto>> LoginUser([FromBody] UserLoginDto loginDto)
        {
            // Handled by global exception middleware; no local try-catch required.
            var response = await _authService.LoginUser(loginDto);
            return Ok(response);
        }
    }
}