
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;

namespace Service.Services;

public class ClassificationDataCache(IServiceScopeFactory scopeFactory) : IClassificationDataCache
{

    public Dictionary<int, Dictionary<int, int>> CategoryTagCounts { get; private set; } = [];
    public List<Category> AllCategories { get; private set; } = [];
    public int VocabularySize { get; private set; }
    public int TotalSongs { get; private set; }

    private HashSet<int> _allUniqueTags = [];

    /// <summary>
    /// Initial loading of all required classification data using a repository layer.
    /// </summary>
    public void Initialize()
    {

        using (var scope = scopeFactory.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IClassificationRepository>();

            // 1. Fetch categories and total song counts via repository
            AllCategories = repository.GetAllCategoriesWithSongs();
            TotalSongs = repository.GetTotalSongsCount();

            // 2. Build the unique tag HashSet from the repository data
            var allTags = repository.GetAllTagIds();
            _allUniqueTags = [.. allTags];
            VocabularySize = _allUniqueTags.Count;

            // 3. Construct the nested cache dictionary using frequencies loaded from the repository
            var frequencies = repository.GetSongTagFrequencies();
            CategoryTagCounts = frequencies
            .AsEnumerable() 
            .GroupBy(stf => stf.Song?.CategoryID ?? 0) 
            .ToDictionary(
                g => g.Key, 
                g => g.GroupBy(stf => stf.TagID)
                      .ToDictionary(
                          tg => tg.Key,
                          tg => tg.Sum(stf => stf.Frequency)
                      )
            );
        }
        
    }

    /// <summary>
    /// Real-time update of the in-memory data when a new song is added to the system.
    /// </summary>
    public void UpdateCacheWithNewSong(int categoryId, List<int> tagIds)
    {
        // 1. Update the total song counter for the prior probability calculation
        TotalSongs++;

        // 2. Retrieve or create the dictionary entry for the specified category
        if (!CategoryTagCounts.ContainsKey(categoryId))
        {
            CategoryTagCounts[categoryId] = [];
        }

        var tagsInCat = CategoryTagCounts[categoryId];

        // 3. Iterate through each tag to update frequencies and system vocabulary size
        foreach (var tagId in tagIds)
        {
            if (tagsInCat.ContainsKey(tagId))
            {
                tagsInCat[tagId]++;
            }
            else
            {
                tagsInCat[tagId] = 1;
            }

            if (_allUniqueTags.Add(tagId))
            {
                VocabularySize++;
            }
        }
    }
}