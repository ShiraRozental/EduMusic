using Microsoft.AspNetCore.Mvc;
using Repository.Entities;
using Repository.Interfaces;

namespace EduMusic.Api.Controllers;

[ApiController] 
[Route("api/[controller]")] 
public class JobsController(IJobRepository jobRepo) : ControllerBase
{

    [HttpGet("{id}")]
    public async Task<IActionResult> GetJobStatus(Guid id)
    {
        var job = await jobRepo.GetByIdAsync(id);
        if (job == null) return NotFound();

        if (job.Status == JobStatus.Failed)
            return Ok(new { Status = "Failed", job.ErrorMessage });

        if (job.Status == JobStatus.Completed)
            return Ok(new
            {
                Status = "Completed",
                job.Song.RawLyrics,
                Category = job.Song.Category?.CategoryName
            });

        return Ok(new { Status = job.Status.ToString() });
    }
}