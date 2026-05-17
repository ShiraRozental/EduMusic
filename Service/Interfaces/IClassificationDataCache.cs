using Repository.Entities;
using Repository.Interfaces;

namespace Service.Interfaces
{
    public interface IClassificationDataCache
    {
        // CategoryID -> (TagID -> Frequency)
        Dictionary<int, Dictionary<int, int>> CategoryTagCounts { get; }
        int VocabularySize { get; }
        int TotalSongs { get; }
        List<Category> AllCategories { get; }

        // Method to update the cache when a new song is added
        void UpdateCacheWithNewSong(int categoryId, List<int> tagIds);

        // Initial load of data from the database
        void Initialize();
    }
}
