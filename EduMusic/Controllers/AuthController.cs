using AutoMapper;
using Common.Dto;
using Microsoft.AspNetCore.Mvc;
using Repository.Entities;
using Service.Interfaces;
using System;

namespace EduMusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        // POST: api/auth/register-admin
        [HttpPost("register-admin")]
        public async Task<ActionResult<AdminDto>> RegisterAdmin(AdminRegisterDto registerDto)
        {
            try
            {
                var admin = await _authService.RegisterAdmin(registerDto);
                return Ok(admin);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/auth/login-admin
        [HttpPost("login-admin")]
        public async Task<ActionResult<AuthResponseDto>> LoginAdmin([FromBody] AdminLoginDto loginDto)
        {
            try
            {
                var response = await _authService.LoginAdmin(loginDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // POST: api/auth/login-user
        [HttpPost("login-user")]
        public async Task<ActionResult<AuthResponseDto>> LoginUser([FromBody] UserLoginDto loginDto)
        {
            try
            {
                var response = await _authService.LoginUser(loginDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }



    }
}
