using Microsoft.Extensions.Configuration;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class GroqApiClient : IGroqApiClient
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public GroqApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["Groq:ApiKey"] ?? throw new InvalidOperationException("Missing Groq API Key");
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> TranscribeAsync(string audioFilePath, string language = "he", CancellationToken ct = default)
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

        public async Task<string> ChatAsync(string systemPrompt, string userMessage, CancellationToken ct = default)
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
