
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;

namespace Service.Services;
/// <summary>
/// Loads and caches all data needed by ClassificationService for one admin.
/// </summary>
public class ClassificationDataCache(IServiceScopeFactory scopeFactory) : IClassificationDataCache
{
    // CategoryID -> (TagID -> Frequency [across all songs in that category])
    public Dictionary<int, Dictionary<int, int>> CategoryTagCounts { get; private set; } = [];

    // Key = CategoryID, Value = How many songs belong to the category
    public Dictionary<int, int> SongsPerCategory { get; private set; } = [];

    // All categories that exist in the system
    public List<Category> AllCategories { get; private set; } = [];

    // total distinct tags 
    public int VocabularySize { get; private set; }

    // Total songs in the system
    public int TotalSongs { get; private set; }

    // Internal set used to detect new tags and update VocabularySize
    private HashSet<int> _allUniqueTags = [];


    // ── INITIALIZATION ───────────────────────────────────────────────────────

    /// <summary>
    /// Initial loading of all required classification data using a repository layer.
    /// </summary>
    public void Initialize()
    {

        using (var scope = scopeFactory.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IClassificationRepository>();

            // 1. All categories in the system
            AllCategories = repository.GetAllCategories();

            // 2. Total songs and per-category song counts (for prior probability)
            TotalSongs = repository.GetTotalSongsCount();
            SongsPerCategory = repository.GetSongsCountPerCategory();

            // 3. Vocabulary size (for Laplace smoothing denominator)
            _allUniqueTags = [.. repository.GetAllTagIds()];
            VocabularySize = _allUniqueTags.Count;

            // 4. CategoryTagCounts: for each category, how many times each tag
            //    CategoryTagCounts[categoryId][tagId] = total frequency
            var frequencies = repository.GetSongTagFrequencies();

            CategoryTagCounts = frequencies
           .AsEnumerable()
           .Where(stf => stf.Song?.CategoryID != null)
           .GroupBy(stf => stf.Song.CategoryID!.Value)       // group by category
           .ToDictionary(
               g => g.Key,
               g => g.GroupBy(stf => stf.TagID)              // within category: group by tag
                     .ToDictionary(
                         tg => tg.Key,
                         tg => tg.Sum(stf => stf.Frequency)  // sum all occurrences
                     )
           );
        }
        
    }

    // ── REAL-TIME UPDATE ─────────────────────────────────────────────────────

    /// <summary>
    /// Updates in-memory cache after a new song is classified and saved to the DB.
    /// Keeps TotalSongs, SongsPerCategory, CategoryTagCounts and VocabularySize current.
    /// </summary>
    public void UpdateCacheWithNewSong(int categoryId, Dictionary<int, int> tagFrequencies)
    {
        // 1. Update the total song counter for the prior probability calculation
        TotalSongs++;

        // 2. One more song in this category → prior numerator grows
        if (SongsPerCategory.ContainsKey(categoryId))
            SongsPerCategory[categoryId]++;
        else
            SongsPerCategory[categoryId] = 1;

        // 3. Ensure this category has an entry in CategoryTagCounts
        if (!CategoryTagCounts.ContainsKey(categoryId))
            CategoryTagCounts[categoryId] = [];

        var tagsInCat = CategoryTagCounts[categoryId];

        // 4. Update each tag's frequency and grow vocabulary if tag is new
        foreach (var (tagId, frequency) in tagFrequencies)
        {
            if (tagsInCat.ContainsKey(tagId))
            {
                tagsInCat[tagId] += frequency;
            }
            else
            {
                tagsInCat[tagId] = frequency;
            }

            // If this tag is new to the system, grow the vocabulary
            if (_allUniqueTags.Add(tagId))
            {
                VocabularySize++;
            }
        }
    }
}