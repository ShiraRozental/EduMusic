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
                //the demucs python server
                //await _jobRepo.UpdateStatusAsync(jobId, JobStatus.SeparatingVocals);
                //vocalsPath = await _separator.SeparateVocalsAsync(filePath, ct);
                vocalsPath = filePath;
                //שלב 1
                await _jobRepo.UpdateStatusAsync(jobId, JobStatus.Transcribing);
                string rawLyrics = await _groq.TranscribeAsync(vocalsPath, "he", ct);

                //שלב 2
                await _jobRepo.UpdateStatusAsync(jobId, JobStatus.FixingLyrics);
                string fixPrompt = """
                        אתה עורך שירים עבריים. תפקידך לתקן שגיאות כתיב ותמלול בלבד.
                        חוקים מחייבים:
                        1. החזר את השיר המלא - כולל כל החזרות, כל הפזמונים, כל הבתים, בדיוק כמו המקור
                        2. אל תקצר, אל תמחק חזרות, אל תכתוב "[פזמון חוזר]" או כל קיצור אחר
                        3. תקן רק שגיאות כתיב ותמלול ברורות
                        4. שמור על מבנה שורות השיר המקורי
                        5. החזר את הטקסט בלבד, ללא הסברים
                        """;
                string cleanLyrics = await _groq.ChatAsync(fixPrompt, rawLyrics, ct);


                // שלב 3: נרמול מילים — קריאה נפרדת, מחזיר רשימה עם כפילויות
                await _jobRepo.UpdateStatusAsync(jobId, JobStatus.NormalizingWords);

                string normalizePrompt = """
                    אתה כלי המרה מורפולוגי לעברית. עבור כל מילה בטקסט:
                    - החזר את צורתה הבסיסית (לֶמָה)
                    - רבים → יחיד: ילדים → ילד
                    - פועל מוטה → שם פועל: ישנתי → לישון, מאיר → להאיר
                    - נסמך → בסיסי: ביתו → בית, שלי → של (השאר כמו שהיא)
    
                    חובה:
                    - עבד כל מילה בטקסט, ללא דילוג
                    - הסר תחיליות דבוקות (ו,ב,ל,כ,מ,ש,ה) מכל מילה לפני הנרמול
                    - שמור על סדר המילים המקורי
                    - שמור כפילויות (מילה שחוזרת תופיע שוב)
                    - החזר JSON בלבד בפורמט: {"words": ["מילה1","מילה2",...]}
                    - אסור להשמיט מילים — גם מילות קישור, גם קריאות, גם כינויי גוף
                    """;
                string wordsJson = await _groq.ChatAsync(normalizePrompt, cleanLyrics, ct);
                List<string> allWords = ParseWordsJson(wordsJson);

                List<string> normalizedWords = FilterStopwords(allWords);


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

        private List<string> ParseWordsJson(string json)
        {
            try
            {
                string cleaned = System.Text.RegularExpressions.Regex.Replace(
                    json, @"```json?|```", "").Trim();

                using var doc = System.Text.Json.JsonDocument.Parse(cleaned);
                return doc.RootElement
                          .GetProperty("words")
                          .EnumerateArray()
                          .Select(e => e.GetString() ?? "")
                          .Where(w => !string.IsNullOrWhiteSpace(w))
                          .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse words JSON: {Json}", json);
                return [];
            }
        }

        private static readonly HashSet<string> HebrewStopwords =
[
            // מילות יחס
            "של", "את", "עם", "על", "אל", "מן", "מ", "ל", "ב", "כ",
            "לפי", "בגלל", "כדי", "בשביל", "אחרי", "לפני", "בין", "אצל",
            // מילות חיבור
            "כי", "אבל", "או", "גם", "רק", "אם", "כש", "כאשר", "אז",
            "לכן", "אבל", "אלא", "אך", "ואף", "בכל", "שוב",
            // כינויי גוף
            "אני", "אתה", "את", "הוא", "היא", "אנחנו", "אתם", "הם", "הן",
            "אנו", "אותי", "אותך", "אותו", "אותה", "אותנו", "אותם",
            // מילות שאלה
            "מה", "מי", "איך", "איפה", "מתי", "למה", "למה", "כמה",
            // מילים חסרות משמעות
            "זה", "זאת", "זו", "הזה", "הזאת", "כן", "לא", "כבר",
            "פתאום", "עוד", "יש", "אין", "היה", "יהיה",
            // קריאות וצלילים
            "היי", "ביי", "הו", "אה", "אוי", "אי", "נה", "לה", "אהי",
            "איי", "הא", "אהה", "ממ", "אממ",
];

        private static List<string> FilterStopwords(List<string> words)
        {
            return words
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Where(w => w.Length > 1)                          // מסנן אותיות בודדות
                .Where(w => !HebrewStopwords.Contains(w))          // מסנן stopwords
                .Where(w => System.Text.RegularExpressions.Regex.IsMatch(w, @"[\u0590-\u05FF]")) // רק מילים עם עברית
                .ToList();
        }
    }
}
