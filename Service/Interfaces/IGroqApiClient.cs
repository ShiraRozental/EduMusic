using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IGroqApiClient
    {
        Task<string> TranscribeAsync(string audioFilePath, string language = "he", CancellationToken ct = default);
        Task<string> ChatAsync(string systemPrompt, string userMessage, CancellationToken ct = default);
    }
}
