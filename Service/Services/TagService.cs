using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;

namespace Service.Services;

public class TagService(ITagRepository tagRepository) : ITagService
{
    // Vocal and musical noise words (the Python service already handles standard Hebrew stopwords)
    private static readonly HashSet<string> LyricsNoiseWords =
    [
        "היי", "ביי", "הו", "אה", "אוי", "אי", "נה", "לה", "אהי",
        "איי", "הא", "אהה", "ממ", "אממ", "hey", "bye"
    ];

    /// <summary>
    /// Filters out lyrics noise and syncs unique tags into the database using the generic repository.
    /// Returns a Dictionary mapping the saved Tag entity to its frequency in the song.
    /// </summary>
    public async Task<Dictionary<Tag, int>> ProcessAndSyncTagsAsync(Dictionary<string, int> wordCounts)
    {
        var finalTagsWithCounts = new Dictionary<Tag, int>();

        foreach (var kvp in wordCounts)
        {
            string word = kvp.Key;
            int count = kvp.Value;

            // 1. Filter out empty items and single characters
            if (string.IsNullOrWhiteSpace(word) || word.Length <= 1)
                continue;

            // 2. Filter out lyrical background noises
            if (LyricsNoiseWords.Contains(word))
                continue;

            // 3. Ensure only words containing Hebrew characters are processed
            if (!Regex.IsMatch(word, @"[\u0590-\u05FF]"))
                continue;

            // 4. Retrieve the tag using the existing generic GetAll method with a filter expression
            var existingTags = await tagRepository.GetAll(t => t.TagText == word);
            var tag = existingTags.FirstOrDefault();

            if (tag == null)
            {
                // 5. Create and add the new tag via the repository (AddItem automatically handles ctx.Save)
                tag = await tagRepository.AddItem(new Tag { TagText = word });
            }

            // 6. Store the tag entity mapped to its frequency count in the current song
            finalTagsWithCounts[tag] = count;
        }

        return finalTagsWithCounts;
    }
}