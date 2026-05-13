using Repository.Interfaces;
using Service.Interfaces;

namespace EduMusic.Background;

/// <summary>
/// Background worker שרץ ברקע האפליקציה.
/// הוא דוגם את מסד הנתונים כל כמה שניות ומחפש משימות חדשות בסטטוס Queued.
/// </summary>
public class LyricsWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<LyricsWorker> _logger;
    private readonly int _pollingDelayMilliseconds = 2000; // זמן המתנה בין בדיקות (2 שניות)

    public LyricsWorker(
        IServiceProvider services,
        ILogger<LyricsWorker> logger,
        IConfiguration config)
    {
        _services = services;
        _logger = logger;
        // ניתן להגדיר את זמן ההמתנה גם דרך ה-appsettings
        _pollingDelayMilliseconds = config.GetValue("Worker:PollingDelayMs", 2000);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LyricsWorker started and polling every {Delay}ms.", _pollingDelayMilliseconds);

        // הלולאה הראשית שתרוץ כל עוד האפליקציה חיה
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // יצירת Scope חדש - קריטי לעבודה עם DbContext/Repositories בתוך Singleton
                using (var scope = _services.CreateScope())
                {
                    // שליפת השירותים מתוך ה-Scope הנוכחי
                    var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                    var processor = scope.ServiceProvider.GetRequiredService<ILyricsProcessor>();

                    // 1. בדיקה אם יש ג'וב שממתין בתור (סטטוס Queued)
                    var job = await jobRepo.GetNextQueuedJobAsync();

                    if (job != null)
                    {
                        _logger.LogInformation("Worker picked up job {JobId} ({FileName})", job.Id, job.OriginalFileName);

                        // 2. הפעלת תהליך העיבוד (הפרדה, תמלול וכו')
                        // הערה: ProcessAsync יעדכן בעצמו את הסטטוסים ב-DB דרך ה-JobRepo
                        await processor.ProcessAsync(job.Id, job.FilePath, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in LyricsWorker loop.");
            }

            // 3. המתנה לפני הבדיקה הבאה ב-DB
            await Task.Delay(_pollingDelayMilliseconds, stoppingToken);
        }

        _logger.LogInformation("LyricsWorker is shutting down.");
    }
}