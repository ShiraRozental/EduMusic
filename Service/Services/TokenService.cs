using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repository.Entities;
using Service.Interfaces;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Service.Services
{
    public class TokenService(IConfiguration config) : ITokenService
    {
        private readonly IConfiguration _config = config;

        public string GenerateAdminToken(Admin admin)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, admin.AdminID.ToString()),
            new Claim(ClaimTypes.Email,           admin.Email),
            new Claim(ClaimTypes.Name,            admin.FullName),
            new Claim(ClaimTypes.Role,            "Admin")   
        };
            return BuildToken(claims, DateTime.UtcNow.AddMonths(1));
        }

        public string GenerateUserToken(User user)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new Claim("NationalID",              user.ID),          
            new Claim("TeacherID",               user.MyTeacherID.ToString()),
            new Claim(ClaimTypes.Role,           "User")    
        };
            return BuildToken(claims, DateTime.UtcNow.AddYears(1));
        }

        private string BuildToken(List<Claim> claims, DateTime expires) 
        {
            var key = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,       
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}


