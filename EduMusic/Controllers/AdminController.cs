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

        [HttpPost("upload-users")]
        public async Task<IActionResult> UploadStudents(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("אנא העלי קובץ אקסל תקין.");

            var adminIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (adminIdClaim == null) return Unauthorized();

            int adminId = int.Parse(adminIdClaim.Value);

            try
            {
                await _userService.ImportUsersFromExcelAsync(file, adminId);
                return Ok(new { message = "רשימת התלמידים נקלטה בהצלחה." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"שגיאה בעיבוד הקובץ: {ex.Message}" });
            }
        }

        
        [HttpPost("add-multiple-manual")]
        public async Task<IActionResult> AddMultiple([FromBody] List<UserProvisioningDto> dtos)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            try
            {
                await _userService.AddUsersManualAsync(dtos, adminId);
                return Ok(new { message = "כל התלמידים נוספו בהצלחה" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/Admin/5/users
        [HttpGet("{teacherId}/users")]
        public async Task<ActionResult<IEnumerable<UserProvisioningDto>>> GetUsers(int teacherId)
        {
            var users = await _service.GetUsersByTeacherId(teacherId);
            return Ok(users);
        }

        // GET api/<AdminController>/5
        [HttpGet("{id}")]
        public async Task<AdminDto> Get(int id)
        {
            return await _service.GetById(id);
        }

        // PUT api/<AdminController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult<AdminDto>> Put(int id, [FromBody] AdminDto adminDto)
        {
            var updatedAdmin = await _service.UpdateItem(id, adminDto);
            return Ok(updatedAdmin);
        }
    }
}
