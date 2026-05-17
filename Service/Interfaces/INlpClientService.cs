using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Interfaces;

public interface INlpClientService
{
    /// <summary>
    /// Sends raw text to the NLP pipeline and extracts the base lemma form of each word.
    /// </summary>
    Task<Dictionary<string, int>> NormalizeLyricsAsync(string lyrics, CancellationToken ct);
}