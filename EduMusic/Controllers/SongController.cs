using Common.Dto;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EduMusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongController : ControllerBase
    {
        private readonly IService<SongDto> _service;

        public SongController( IService<SongDto> service) 
        {
            _service = service;
        }

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
    }
}
