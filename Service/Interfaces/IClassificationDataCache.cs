using Repository.Entities;
using Repository.Interfaces;

namespace Service.Interfaces
{
    public interface IClassificationDataCache
    {
        // CategoryID -> (TagID -> Frequency)
        Dictionary<int, Dictionary<int, int>> CategoryTagCounts { get; }

        // Key = CategoryID, Value = How many songs belong to the category
        Dictionary<int, int> SongsPerCategory { get; }
        List<Category> LeafCategories { get; }
        int VocabularySize { get; }
        int TotalSongs { get; }

        // Initial load of data from the database
        void Initialize();
        // Method to update the cache when a new song is added
        void UpdateCacheWithNewSong(int categoryId, Dictionary<int, int> tagFrequencies);

        void ReassignSong(int oldCategoryId, int newCategoryId, Dictionary<int, int> tagFrequencies);

    }
}
