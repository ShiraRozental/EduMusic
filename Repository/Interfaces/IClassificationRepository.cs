 using Repository.Entities; 
namespace Repository.Interfaces
{
    /// <summary>
    /// Defines data-access operations required to populate and sync the classification cache.
    /// </summary>
    public interface IClassificationRepository
    {
        /// <summary>
        /// Retrieves all categories from the database, including their related songs.
        /// </summary>
        List<Category> GetAllCategoriesWithSongs();

        /// <summary>
        /// Gets the total count of songs currently stored in the system.
        /// </summary>
        int GetTotalSongsCount();

        /// <summary>
        /// Retrieves a unique list of all Tag IDs existing across the entire database.
        /// </summary>
        List<int> GetAllTagIds();

        /// <summary>
        /// Retrieves all song-tag frequency records, including eagerly loaded song details.
        /// </summary>
        List<SongTagFrequency> GetSongTagFrequencies();
    }
}