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
    public class AdminController(IAdminService service, IUserService userServ) : ControllerBase
    {
        private readonly IAdminService _service = service;
        private readonly IUserService _userService = userServ;

        // POST: api/Admin/upload-users
        [HttpPost("upload-users")]
        public async Task<IActionResult> UploadStudents(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a valid Excel file.");

            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (adminIdClaim == null) return Unauthorized();

            int adminId = int.Parse(adminIdClaim.Value);
            try
            {
                await _userService.ImportUsersFromExcelAsync(file, adminId);
                return Ok(new { message = "Student list imported successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Error processing file: {ex.Message}" });
            }
        }

        // POST: api/Admin/add-multiple-manual
        [HttpPost("add-multiple-manual")]
        public async Task<IActionResult> AddMultiple([FromBody] List<UserProvisioningDto> dtos)
        {
            int adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            try
            {
                await _userService.AddUsersManualAsync(dtos, adminId);
                return Ok(new { message = "All students added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/Admin/{teacherId}/users
        [HttpGet("{teacherId}/users")]
        public async Task<ActionResult<IEnumerable<UserProvisioningDto>>> GetUsers(int teacherId)
        {
            var users = await _service.GetUsersByTeacherId(teacherId);
            return Ok(users);
        }

        // GET: api/Admin/{id}
        [HttpGet("{id}")]
        public async Task<AdminDto> Get(int id)
        {
            return await _service.GetById(id);
        }

        // PUT: api/Admin/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<AdminDto>> Put(int id, [FromBody] AdminDto adminDto)
        {
            var updatedAdmin = await _service.UpdateItem(id, adminDto);
            return Ok(updatedAdmin);
        }
    }
}