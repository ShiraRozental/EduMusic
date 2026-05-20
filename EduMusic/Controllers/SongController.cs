using Common.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using System.Security.Claims;

namespace EduMusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongController(ISongService songService) : ControllerBase
    {

        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> Upload([FromForm] SongUploadDto dto)
        {
            int uploaderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var song = await songService.UploadAndSaveSongAsync(dto, uploaderId);
            return Ok(new { song.SongID, song.Title, song.Status });
        }
        /*
        // GET: api/<SongController>
        [HttpGet]
        public async Task<List<SongDto>> Get()
        {
            return await _service.GetAll();
        }

        // GET api/<SongController>/5
        [HttpGet("{id}")]
        public async Task<SongDto> Get(int id)
        {
            return await _service.GetById(id);
        }

        //CHECK
        // POST api/<SongController>
        [HttpPost]
        public async Task<SongDto> Post([FromBody] string value)
        {
            return await _service.AddItem(new SongDto { Title = value, Artist = "Unknown", Duration = 0, FilePath = "path/to/file", CategoryID = 1, UploaderID = 1 });
        }

        // PUT api/<SongController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<SongController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
        */
    }
}
