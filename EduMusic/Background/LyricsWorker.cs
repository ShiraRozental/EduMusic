using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;

namespace EduMusic.Background;

/// <summary>
/// Background worker that runs in the background of the application.
/// It samples the database every few seconds and looks for new tasks in Queued status.
/// </summary>
public class LyricsWorker(IServiceProvider services, ILogger<LyricsWorker> logger, IConfiguration config) : BackgroundService
{
    private readonly IServiceProvider _services = services;
    private readonly ILogger<LyricsWorker> _logger = logger;
    private readonly int _pollingDelayMilliseconds = config.GetValue("Worker:PollingDelayMs", 2000);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LyricsWorker started and polling every {Delay}ms.", _pollingDelayMilliseconds);

        // The main loop that will run as long as the app is alive
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _services.CreateScope())
                {
                    // Retrieve the services from the current scope
                    var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                    var processor = scope.ServiceProvider.GetRequiredService<ILyricsProcessor>();

                    // 1. Check if there is a job waiting in the queue (Queued status)
                    var job = await jobRepo.GetNextQueuedJobAsync();

                    if (job != null)
                    {
                        await jobRepo.UpdateStatusAsync(job.Id, JobStatus.SeparatingVocals);

                        _logger.LogInformation("Worker picked up job {JobId}", job.Id);

                        // 2. Running the processing process (separation, transcription, etc.)
                        await processor.ProcessAsync(job.Id, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in LyricsWorker loop.");
            }

            // 3. Waiting before the next DB check
            await Task.Delay(_pollingDelayMilliseconds, stoppingToken);
        }

        _logger.LogInformation("LyricsWorker is shutting down.");
    }
}