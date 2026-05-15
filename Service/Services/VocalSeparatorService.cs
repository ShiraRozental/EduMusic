using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using System.Net.Http;
using System.Net.Http.Headers;

 
namespace Service.Services
{
    /// <summary>
    /// Service responsible for communicating with the Demucs Python API 
    /// to perform audio source separation.
    /// </summary>
    public class VocalSeparatorService(
    ILogger<VocalSeparatorService> logger,
    IConfiguration config,
    IHttpClientFactory httpClientFactory) : IVocalSeparatorService
    {

        private readonly ILogger<VocalSeparatorService> _logger = logger;
        private readonly HttpClient _http = httpClientFactory.CreateClient("DemucsApi");

        private readonly string _baseUrl = config["Demucs:ApiBaseUrl"]?.TrimEnd('/')
                                           ?? "http://localhost:8000";

        private readonly string _outputDir = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "demucs_out")).FullName;


        /// <summary>
        /// Uploads the audio file via a multipart POST request and downloads the extracted vocal stream.
        /// </summary>
        public async Task<string> SeparateVocalsAsync(string inputFilePath, CancellationToken ct = default)
        {
            // 1. Validation: Ensure the source file actually exists on the disk
            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException("Input audio file not found.", inputFilePath);

            _logger.LogInformation(
                "Sending '{File}' to Demucs API at {Url}",
                Path.GetFileName(inputFilePath), _baseUrl);

            // 2. Preparation: Open file stream and initialize multipart form content
            await using var fileStream = File.OpenRead(inputFilePath);
            using var content = new MultipartFormDataContent();

            // 3. Wrapping: Package the stream with appropriate MIME type headers
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(GetMimeType(inputFilePath));

            // 4. Payload: Add the file content to the form under the key "file"
            content.Add(fileContent, "file", Path.GetFileName(inputFilePath));

            // 5. Execution: Send the POST request to the Python Microservice
            var response = await _http.PostAsync(
                $"{_baseUrl}/separate/vocals",
                content, ct);

            // 6. Error Handling: Check if the API request succeeded
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError(
                    "Demucs API returned {Status}: {Error}",
                    response.StatusCode, error);
                throw new InvalidOperationException(
                    $"Vocal separation failed ({response.StatusCode}): {error}");
            }

            // 7. Identification: Extract Job ID from response headers for file naming
            var jobId = response.Headers.TryGetValues("X-Job-Id", out var vals)
                         ? vals.First()
                         : Guid.NewGuid().ToString("N");

            // 8. Storage: Save the binary response (the vocal WAV) to the local temp directory
            var localPath = Path.Combine(_outputDir, $"{jobId}_vocals.wav");

            var wavBytes = await response.Content.ReadAsByteArrayAsync(ct);
            await File.WriteAllBytesAsync(localPath, wavBytes, ct);

            _logger.LogInformation("Vocals saved locally: {Path}", localPath);

            // 9. Return: Provide the path to the newly created local file for the next pipeline stage
            return localPath;
        }

        /// <summary>
        /// Performs a cleanup of the local temporary file to free up disk space.
        /// </summary>
        public void CleanupOutput(string vocalsPath)
        {
            try
            {
                if (File.Exists(vocalsPath))
                    File.Delete(vocalsPath);

                _logger.LogInformation("Cleaned up: {Path}", vocalsPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean up: {Path}", vocalsPath);
            }
        }

        /// <summary>
        /// Sends a DELETE request to the remote API to purge job-related data on the server side.
        /// </summary>
        public async Task DeleteRemoteJobAsync(string jobId, CancellationToken ct = default)
        {
            try
            {
                await _http.DeleteAsync($"{_baseUrl}/jobs/{jobId}", ct);
                _logger.LogInformation("Remote job {JobId} deleted.", jobId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete remote job {JobId}", jobId);
            }
        }

        // ─────────────────────────────────────────────
        private static string GetMimeType(string path) =>
            Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".mp3" => "audio/mpeg",
                ".flac" => "audio/flac",
                ".ogg" => "audio/ogg",
                ".m4a" => "audio/mp4",
                _ => "audio/wav",
            };
    }
}

