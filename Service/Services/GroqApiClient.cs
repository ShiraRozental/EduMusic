using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Service.Services
{
    // ─── DTOs ────────────────────────────────────────────────────────────────
    public class WhisperSegment
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("start")]
        public double Start { get; set; }

        [JsonPropertyName("end")]
        public double End { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        // avg_logprob: confidence score per segment — low = unreliable transcription
        [JsonPropertyName("avg_logprob")]
        public double AvgLogProb { get; set; }

        // no_speech_prob: probability that segment is silence/noise
        [JsonPropertyName("no_speech_prob")]
        public double NoSpeechProb { get; set; }
    }

    public class WhisperVerboseResponse
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("segments")]
        public WhisperSegment[]? Segments { get; set; }

        [JsonPropertyName("duration")]
        public double Duration { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; } = string.Empty;
    }

    // ─── Service ─────────────────────────────────────────────────────────────

    public class GroqApiClient : IGroqApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<GroqApiClient> _logger;

        private const string WhisperModel = "whisper-large-v3";
        private const string ChatModel = "llama-3.3-70b-versatile";
        private const string GroqTranscribeUrl = "https://api.groq.com/openai/v1/audio/transcriptions";
        private const string GroqChatUrl = "https://api.groq.com/openai/v1/chat/completions";

        // confidence thresholds for segment filtering
        private const double MinLogProb = -1.0;
        private const double MaxNoSpeechProb = 0.6;
        private const double MinCoverageRatio = 0.60;
        private const double MaxGapSeconds = 4.0;
        public GroqApiClient(HttpClient http, IConfiguration config, ILogger<GroqApiClient> logger)
        {
            _http = http;
            _logger = logger;

            var apiKey = config["Groq:ApiKey"]
                ?? throw new InvalidOperationException("Missing Groq API Key");

            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            _http.Timeout = TimeSpan.FromMinutes(10);
        }

        // ─── Transcription ────────────────────────────────────────────────────

        public async Task<string> TranscribeAsync(
           string audioFilePath,
           string language = "he",
           CancellationToken ct = default)
        {
            if (!File.Exists(audioFilePath))
                throw new FileNotFoundException("Audio file not found", audioFilePath);

            _logger.LogInformation("Starting transcription: {File}", Path.GetFileName(audioFilePath));

            return await RetryAsync(3, async () =>
            {
                await using var fileStream = File.OpenRead(audioFilePath);

                using var form = new MultipartFormDataContent();

                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(
                        GetAudioMimeType(audioFilePath));

                form.Add(streamContent, "file", Path.GetFileName(audioFilePath));
                form.Add(new StringContent(WhisperModel), "model");
                form.Add(new StringContent(language), "language");

                form.Add(new StringContent("verbose_json"), "response_format");

                // temperature=0 forces deterministic output, reduces hallucinations
                form.Add(new StringContent("0"), "temperature");

                
                var response = await _http.PostAsync(GroqTranscribeUrl, form, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("Whisper error {Status}: {Error}", response.StatusCode, error);
                    response.EnsureSuccessStatusCode();
                }

                var json = await response.Content
                    .ReadFromJsonAsync<WhisperVerboseResponse>(cancellationToken: ct)
                    ?? throw new InvalidOperationException("Empty transcription response");

                return ReconstructFromSegments(json, audioFilePath);

            }, ct);
        }

        // ─── Chat ─────────────────────────────────────────────────────────────

        public async Task<string> ChatAsync(
            string systemPrompt,
            string userMessage,
            CancellationToken ct = default)
        {
            _logger.LogInformation("ChatAsync: {Chars} chars input", userMessage.Length);

            var payload = new
            {
                model = ChatModel,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user",   content = userMessage  }
                },
                temperature = 0.1,  
                max_tokens = 4096
            };

            return await RetryAsync(3, async () =>
            {
                var response = await _http.PostAsJsonAsync(GroqChatUrl, payload, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("Groq chat error {Status}: {Error}", response.StatusCode, error);
                    response.EnsureSuccessStatusCode();
                }

                var result = await response.Content
                    .ReadFromJsonAsync<JsonElement>(cancellationToken: ct);

                return result
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? string.Empty;
            }, ct);
        }

        // ─── Segment reconstruction ───────────────────────────────────────────

        private string ReconstructFromSegments(WhisperVerboseResponse response, string filePath)
        {
            if (response.Segments is null || response.Segments.Length == 0)
            {
                _logger.LogWarning("No segments for {File} — falling back to raw text",
                    Path.GetFileName(filePath));
                return response.Text ?? string.Empty;
            }

            var reliable = response.Segments
                .Where(s => s.AvgLogProb >= MinLogProb && s.NoSpeechProb <= MaxNoSpeechProb)
                .OrderBy(s => s.Start)
                .ToList();

            int dropped = response.Segments.Length - reliable.Count;
            if (dropped > 0)
                _logger.LogWarning("Dropped {N} unreliable segments (noise/silence)", dropped);

            var sb = new StringBuilder();
            double lastEnd = 0;

            foreach (var seg in reliable)
            {
                double gap = seg.Start - lastEnd;

                if (gap > MaxGapSeconds)
                    _logger.LogWarning("Gap {Gap:F1}s at t={Time:F1}s — possible skipped lyrics",
                        gap, seg.Start);

                var text = seg.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(text)) { lastEnd = seg.End; continue; }

                sb.AppendLine(text);

                lastEnd = seg.End;
            }

            double coverage = response.Duration > 0
                ? lastEnd / response.Duration : 1.0;

            _logger.LogInformation(
                "Transcription done: {Segs} segments ({Reliable} reliable), coverage {Cov:P0}, duration {Dur:F0}s",
                response.Segments.Length, reliable.Count, coverage, response.Duration);

            if (coverage < MinCoverageRatio)
                _logger.LogWarning("Low coverage {Cov:P0} — vocals may be mixed with music, consider vocal separation",
                    coverage);

            return sb.ToString().Trim();
        }

        // ─── Helpers ──────────────────────────────────────────────────────────
        private static string GetAudioMimeType(string filePath) =>
           Path.GetExtension(filePath).ToLowerInvariant() switch
           {
               ".mp3" => "audio/mpeg",
               ".wav" => "audio/wav",
               ".flac" => "audio/flac",
               ".m4a" => "audio/mp4",
               ".ogg" => "audio/ogg",
               _ => "application/octet-stream"
           };

        private async Task<T> RetryAsync<T>(
            int maxAttempts,
            Func<Task<T>> action,
            CancellationToken ct)
        {
            int delay = 2;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (HttpRequestException ex) when (attempt < maxAttempts && !ct.IsCancellationRequested)
                {
                    _logger.LogWarning(
                        "Attempt {A}/{M} failed: {Msg}. Retry in {D}s",
                        attempt, maxAttempts, ex.Message, delay);
                    await Task.Delay(TimeSpan.FromSeconds(delay), ct);
                    delay *= 2;
                }
            }
            throw new InvalidOperationException("RetryAsync: all attempts exhausted");
        }

        // ─── Last Models ──────────────────────────────────────────────────────────

        public async Task<string> TranscribeAsyncLast(string audioFilePath, string language = "he", CancellationToken ct = default)
        {
            await using var fileStream = File.OpenRead(audioFilePath);
            using var form = new MultipartFormDataContent();
            form.Add(new StreamContent(fileStream), "file", Path.GetFileName(audioFilePath));
            form.Add(new StringContent("whisper-large-v3"), "model");
            form.Add(new StringContent(language), "language");
            form.Add(new StringContent("text"), "response_format");

            var response = await _http.PostAsync("https://api.groq.com/openai/v1/audio/transcriptions", form, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(ct);
        }

        public async Task<string> ChatAsyncLast(string systemPrompt, string userMessage, CancellationToken ct = default)
        {
            var payload = new
            {
                model = "llama-3.3-70b-versatile",
                messages = new[] {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            },
                temperature = 0.2
            };
            var response = await _http.PostAsJsonAsync("https://api.groq.com/openai/v1/chat/completions", payload, ct);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(cancellationToken: ct);
            return result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
        }
    }
}
