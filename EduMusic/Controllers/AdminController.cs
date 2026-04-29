using Common.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Services;
using System.Security.Claims;

namespace EduMusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController(IService<AdminDto> service) : ControllerBase
    {
        private readonly IService<AdminDto> _service = service;
        private readonly IUserService _userService;

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

        // GET: api/<AdminController>
        [HttpGet]
        public async Task<List<AdminDto>> Get()
        {
            return await _service.GetAll();
        }

        // GET api/<AdminController>/5
        [HttpGet("{id}")]
        public async Task<AdminDto> Get(int id)
        {
            return await _service.GetById(id);
        }

        // POST api/<AdminController>
        [HttpPost]
        public async Task<AdminDto> Post([FromBody] AdminDto admin)
        {
            return await _service.AddItem(admin);
        }

        // PUT api/<AdminController>/5
        [HttpPut("{id}")]
        public async Task<AdminDto> Put(int id, [FromBody] AdminDto admin)
        {
            return await _service.UpdateItem(id, admin);
        }

        // DELETE api/<AdminController>/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            await _service.DeleteItem(id);
        }
    }
}
