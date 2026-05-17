using System.Collections.Generic;
using System.Threading.Tasks;
using Repository.Entities;

namespace Service.Interfaces;

public interface ITagService
{
    /// <summary>
    /// Filters out lyrics noise and syncs unique tags into the database.
    /// Returns a Dictionary mapping the saved Tag entity to its frequency in the song.
    /// </summary>
    /// <param name="wordCounts">The dictionary of base lemmas and their frequencies received from the NLP service.</param>
    /// <returns>A dictionary of synchronized Tag entities mapped to their respective frequency counts.</returns>
    Task<Dictionary<Tag, int>> ProcessAndSyncTagsAsync(Dictionary<string, int> wordCounts);
}