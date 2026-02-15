using Common.Dto;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EduMusic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IService<AdminDto> _service;

        public AdminController(IService<AdminDto> service) 
        { 
            _service = service;
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
            var path = Path.Combine(Environment.CurrentDirectory, "images/", admin.ImageFile.FileName);
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                admin.ImageFile.CopyTo(fs);
                fs.Close();
            }
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
