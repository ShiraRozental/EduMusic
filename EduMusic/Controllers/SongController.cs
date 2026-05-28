using AutoMapper;
using Common.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Entities;
using Service.Interfaces;
using Service.Services;
using System.Security.Claims;

namespace EduMusic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SongController(ISongService songService, IMapper mapper) : ControllerBase
    {
        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> Upload([FromForm] SongUploadDto dto)
        {
            int uploaderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var song = await songService.UploadAndSaveSongAsync(dto, uploaderId);
            return Ok(mapper.Map<SongSearchResultDto>(song));
        }

        [HttpGet("{songId}/status")]
        [Authorize]
        public async Task<IActionResult> GetStatus(int songId)
        {
            var song = await songService.GetSongByIdAsync(songId);
            if (song == null) return NotFound();
            return Ok(mapper.Map<SongSearchResultDto>(song));
        }

        // 3. מסך החיפוש הממוקד (בלי עומס של מילים, רק לפי בחירות מוגדרות)
        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> Search(
            [FromQuery] string? title,
            [FromQuery] string? artist,
            [FromQuery] int? categoryId,
            [FromQuery] int? tagId)
        {
            var results = await songService.SearchAsync(title, artist, categoryId, tagId);
            return Ok(results);
        }

        // 4. שליפת כל התגיות פעם אחת עבור ה-Client-side Autocomplete ב-React
        [HttpGet("tags")]
        [Authorize]
        public async Task<IActionResult> GetAllTags()
        {
            var tags = await songService.GetAllTagsAsync();
            return Ok(mapper.Map<List<TagDto>>(tags));
        }

        [HttpPatch("{songId}/category")]
        [Authorize]
        public async Task<IActionResult> ReassignCategory(int songId, [FromBody] ReassignCategoryDto dto)
        {
            int adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await songService.ReassignCategoryAsync(songId, dto.NewCategoryId, adminId);
            return NoContent();
        }

        [HttpDelete("{songId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSong(int songId)
        {
            var result = await songService.DeleteSongAsync(songId);
            if (!result) return NotFound();
            return NoContent();
        }
       
    }
}
