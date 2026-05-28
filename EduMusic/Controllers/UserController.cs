using Common.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using System.Security.Claims;

namespace EduMusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET /api/User
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            int teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var users = await _userService.GetAllUsersAsync(teacherId);
            return Ok(users);
        }

        // POST /api/User
        [HttpPost]
        public async Task<IActionResult> AddManual([FromBody] IEnumerable<UserProvisioningDto> dtos)
        {
            int teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _userService.AddUsersManualAsync(dtos, teacherId);
            return Ok();
        }

        // DELETE /api/User/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }

        // POST /api/User/import-excel
        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            int teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _userService.ImportUsersFromExcelAsync(file, teacherId);
            return Ok();
        }
    }
}