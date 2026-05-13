using Microsoft.Extensions.Logging;
using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class LyricsProcessor(IVocalSeparatorService separator, IGroqApiClient groq,
                               //ILyricsClassifierService classifier,
                                IJobRepository jobRepo,
                               ILogger<LyricsProcessor> logger) : ILyricsProcessor
    {
        private readonly IVocalSeparatorService _separator = separator;
        private readonly IGroqApiClient _groq = groq;
        //private readonly ILyricsClassifierService _classifier = classifier;
        private readonly IJobRepository _jobRepo = jobRepo;
        private readonly ILogger<LyricsProcessor> _logger = logger;

   

        public async Task ProcessAsync(Guid jobId, string filePath, CancellationToken ct)
        {
            string? vocalsPath = null;
            try
            {
                await _jobRepo.UpdateStatusAsync(jobId, JobStatus.SeparatingVocals);
                vocalsPath = await _separator.SeparateVocalsAsync(filePath, ct);

                await _jobRepo.UpdateStatusAsync(jobId, JobStatus.Transcribing);
                string rawLyrics = await _groq.TranscribeAsync(vocalsPath, "he", ct);

                await _jobRepo.UpdateStatusAsync(jobId, JobStatus.FixingLyrics);
                string cleanLyrics = await _groq.ChatAsync("תקן שגיאות תמלול בשיר העברי הבא, החזר רק טקסט מתוקן.", rawLyrics, ct);

                //await _jobRepo.UpdateStatusAsync(jobId, JobStatus.Classifying);
                //string category = await _classifier.ClassifyAsync(cleanLyrics, ct);

                //await _jobRepo.CompleteJobAsync(jobId, cleanLyrics, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed job {Id}", jobId);
                await _jobRepo.UpdateStatusAsync(jobId, JobStatus.Failed, ex.Message);
            }
            finally
            {
                TryDeleteFile(filePath);
                if (vocalsPath != null) _separator.CleanupOutput(vocalsPath);
            }
        }

        private void TryDeleteFile(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch (Exception ex) { _logger.LogWarning(ex, "Could not delete {Path}", path); }
        }
    }
}
