using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.Interfaces;

namespace Service.Services;

public class NlpClientService(HttpClient httpClient, ILogger<NlpClientService> logger) : INlpClientService
{
    /// <summary>
    /// Sends clean lyrics to the local Python web server and retrieves base lemmas.
    /// </summary>
    public async Task<Dictionary<string, int>> NormalizeLyricsAsync(string lyrics, CancellationToken ct)
    {
        logger.LogInformation("Sending lyrics to Python NLP server for counting. Lyrics length: {Length} chars.", lyrics?.Length ?? 0);

        try
        {
            var response = await httpClient.PostAsJsonAsync("extract", new { text = lyrics }, ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Python NLP server returned an error status: {StatusCode}", response.StatusCode);
                throw new HttpRequestException($"Error connecting to Python NLP service. Status: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<NormalizationCountResponse>(cancellationToken: ct);

            logger.LogInformation("Successfully received {Count} unique words with frequencies.", result?.WordCounts?.Count ?? 0);

            return result?.WordCounts ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during lyrics normalization and counting.");
            throw;
        }
    }
}

public class NormalizationCountResponse
{
    [JsonPropertyName("wordCounts")]
    public Dictionary<string, int> WordCounts { get; set; } = [];
}