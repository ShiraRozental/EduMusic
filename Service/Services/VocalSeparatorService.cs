using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;


 
namespace Service.Services
{
    /// <summary>
    /// Calls the Python Flask microservice (POST /separate-vocals) with a local file path.
    /// The service runs UVR-MDX-NET vocal separation and saves the result to disk.
    /// Returns the path of the vocals-only WAV file produced by the Python service.
    /// Both services must share the same file system for the returned path to be accessible.
    /// </summary>
    public class VocalSeparatorService(HttpClient http) : IVocalSeparatorService
    {
        private readonly HttpClient _http = http;

        public async Task<string> SeparateVocalsAsync(string filePath, CancellationToken ct)
        {
            var body = JsonContent.Create(new { audio_path = filePath });
            var response = await _http.PostAsync("http://localhost:5000/separate-vocals", body, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<VocalsResult>(cancellationToken: ct);
            return result!.VocalsPath;
        }

        private record VocalsResult([property: JsonPropertyName("vocals_path")] string VocalsPath);
    }
}

