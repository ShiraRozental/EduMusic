using Microsoft.Extensions.Logging;
using Repository.Entities;
using Repository.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Common.enums;

namespace Service.Services;

public class LyricsProcessor(IVocalSeparatorService separator,
                            IGroqApiClient groq,
                            IClassificationService classificationService,
                            IJobRepository jobRepo,
                            ITagService tagService,
                            ILogger<LyricsProcessor> logger,
                            INlpClientService nlpClient,
                            ISongRepository songRepo,
                            IWebHostEnvironment environment) : ILyricsProcessor
{
    private readonly IVocalSeparatorService _separator = separator;
    private readonly IGroqApiClient _groq = groq;
    private readonly IClassificationService _classificationService = classificationService;
    private readonly IJobRepository _jobRepo = jobRepo;
    private readonly ITagService _tagService = tagService;
    private readonly ILogger<LyricsProcessor> _logger = logger;
    private readonly INlpClientService _nlpClient = nlpClient;
    private readonly ISongRepository _songRepo = songRepo;
    private readonly IWebHostEnvironment _environment = environment;

    public async Task ProcessAsync(Guid jobId, CancellationToken ct)
    {
        var job = await _jobRepo.GetByIdAsync(jobId); //return job + Song + FilePath
        if (job == null) throw new Exception($"Job {jobId} not found");

        string filePath = Path.Combine(_environment.WebRootPath, job.Song.FilePath);
        try
        {

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Audio file not found: {filePath}");

            job.Song.Status = SongStatus.ExtractingLyrics;
            // ── Step 1: Vocal separation ──────────────────────────────────────
            await _jobRepo.UpdateStatusAsync(jobId, JobStatus.SeparatingVocals);
            string vocalsPath = await _separator.SeparateVocalsAsync(filePath, ct);

            // ── Step 2: Transcribe audio using Groq Whisper API────────────────────────────
            await _jobRepo.UpdateStatusAsync(jobId, JobStatus.Transcribing);
            string rawLyrics = await _groq.TranscribeAsync(vocalsPath, "he", ct);

            // ── Step 3: Normalize raw text before sending to LLM ─────────────
            string normalizedRaw = NormalizeHebrewText(rawLyrics);

            // ── Step 4: Fix spelling and transcription errors via LLM ─────────
            await _jobRepo.UpdateStatusAsync(jobId, JobStatus.FixingLyrics);
            string cleanLyrics = await _groq.ChatAsync(BuildFixPrompt(), normalizedRaw, ct);

            job.Song.Status = SongStatus.Classifying;

            // ── Step 5: Extract lemmas via Python NLP service ─────────────────
            await _jobRepo.UpdateStatusAsync(jobId, JobStatus.NormalizingWords);
            Dictionary<string, int> wordCounts = await _nlpClient.NormalizeLyricsAsync(normalizedRaw, ct);

            // ── Step 6: Sync tags ─────────────────────────────────────────────
            await _jobRepo.UpdateStatusAsync(jobId, JobStatus.SynchronizingTags);
            Dictionary<Tag, int> finalTags = await _tagService.ProcessAndSyncTagsAsync(wordCounts);

            // ── Step 7: Classify ──────────────────────────────────────────────
            await _jobRepo.UpdateStatusAsync(jobId, JobStatus.Classifying);
            var category = _classificationService.PredictCategory(finalTags, job.Song.UploaderID);

            string categoryName = category?.CategoryName ?? "Unknown";
            int? categoryId = category?.CategoryID;

            // ── Step 8: Persist results ───────────────────────────────────────

            await _jobRepo.CompleteJobAsync(jobId);
            await _songRepo.UpdateSongResultAsync(job.SongID, normalizedRaw, categoryId, finalTags);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed job {Id}", jobId);
            await _jobRepo.UpdateStatusAsync(jobId, JobStatus.Failed, ex.Message);
        }
        finally
        {
            _logger.LogInformation("ProcessAsync finished for job {Id}", jobId);
        }
    }

    // ─── Prompt builder ───────────────────────────────────────────────────────
    private static string BuildFixPrompt() => """
         אתה כלי תיקון תמלול לשירים עבריים. קיבלת תמלול גולמי מWhisper שעלול להכיל שגיאות.
        
         המשימה שלך: תקן שגיאות כתיב ותמלול בלבד. אל תשנה תוכן.
        
         חוקים מחייבים:
         1. החזר את השיר המלא — כל בית, כל פזמון, כל חזרה, בסדר המקורי
         2. אל תכתוב [פזמון חוזר] או כל קיצור — כתוב את הטקסט עצמו
         3. שמור על מבנה השורות המקורי
         4. החזר טקסט בלבד — ללא כותרות, הסברים או סימני פיסוק מיותרים
        
         שגיאות נפוצות שיש לתקן:
         - החלפת אותיות דומות: א↔ה (מהבהב לא מאבהב), ו↔ב, ח↔כ, ס↔ש, צ↔ס, ט↔ת
         - מילים דבוקות שצריך להפריד: "שלילדים" → "של ילדים"
         - מילים מפוצלות שצריך לחבר: "ב כלל" → "בכלל"
         - ניחוש שגוי של מילה נדירה — אם לא בטוח, השאר כמות שהיא
         - חזרות שWhisper דילג עליהן — אם הגיון השיר מצריך חזרה, השלם אותה
        
         אל תתקן:
         - סלנג מכוון או מילים מומצאות שנראות כחלק מהשיר
         - שמות פרטיים
         - מילים שאתה לא בטוח בהן — עדיף להשאיר מאשר לקלקל
         """;

    // ─── Text normalization ───────────────────────────────────────────────────
    private static string NormalizeHebrewText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        // remove Hebrew diacritics (niqqud) — Stanza handles unvocalized text better
        text = Regex.Replace(text, @"[\u05B0-\u05C7]", "");

        // collapse 3+ consecutive newlines to double newline (preserve stanza breaks)
        text = Regex.Replace(text, @"\n{3,}", "\n\n");

        // remove non-Hebrew characters except spaces, newlines, and basic punctuation
        text = Regex.Replace(text, @"[^\p{IsHebrew}\s\n\r""'-.,!?]", " ");
        // collapse multiple spaces
        text = Regex.Replace(text, @" {2,}", " ");

        return text.Trim();
    }
      
}

