using Microsoft.AspNetCore.Mvc;
using Repository.Entities;
using Repository.Interfaces;

namespace EduMusic.Api.Controllers;

[ApiController] // מגדיר את המחלקה כקונטרולר של API
[Route("api/[controller]")] // הכתובת תהיה api/jobs
public class JobsController(IJobRepository jobRepo, IConfiguration config) : ControllerBase
{
    // נתיב בשרת שבו נשמור את הקבצים שהועלו
    private readonly string _uploadPath = config["Storage:UploadPath"] ?? "Uploads";

    [HttpPost("upload")] // מציין שזו פעולת שליחה (POST)
    public async Task<IActionResult> UploadFiles(List<IFormFile> files)
    {
        // 1. בדיקה בסיסית - האם בכלל נשלחו קבצים?
        if (files == null || files.Count == 0)
            return BadRequest("לא נבחרו קבצים.");

        var jobIds = new List<Guid>();

        // 2. מעבר בלולאה על כל הקבצים שהמשתמש שלח
        foreach (var file in files)
        {
            // יצירת מזהה ייחודי (Guid) לכל שיר
            var jobId = Guid.NewGuid();

            // יצירת שם קובץ בטוח (למשל: 550e8400.mp3)
            var extension = Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_uploadPath, $"{jobId}{extension}");

            // 3. שמירת הקובץ הפיזי בתיקייה בשרת
            Directory.CreateDirectory(_uploadPath); // וודוא שהתיקייה קיימת
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 4. יצירת רשומה בבסיס הנתונים
            var newJob = new JobState
            {
                Id = jobId,
                OriginalFileName = file.FileName,
                FilePath = filePath,
                Status = JobStatus.Queued, // הסטטוס ההתחלתי הוא תמיד "בתור"
                CreatedAt = DateTime.UtcNow
            };

            await jobRepo.AddJobAsync(newJob);
            jobIds.Add(jobId);
        }

        // 5. החזרת רשימת המזהים למשתמש
        return Ok(new { Message = "הקבצים הועלו בהצלחה", JobIds = jobIds });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetJobResult(Guid id)
    {
        // שליפת המשימה מהדאטה-בייס לפי ה-ID
        var job = await jobRepo.GetByIdAsync(id);

        if (job == null) return NotFound("המשימה לא נמצאה.");

        // אם המשימה הושלמה, מחזירים את המילים (Lyrics)
        if (job.Status == JobStatus.Completed)
        {
            return Ok(new
            {
                Status = "Completed",
                Lyrics = job.Lyrics,
                Category = job.Category
            });
        }

        // אם היא עדיין בעיבוד, מחזירים רק את הסטטוס הנוכחי
        return Ok(new { Status = job.Status.ToString() });
    }
}