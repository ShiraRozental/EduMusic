
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;

namespace Repository.Repositories;

public class ClassificationRepository(IContext context) : IClassificationRepository
{
    /// <summary>
    /// Retrieves all categories from the database, including their related songs.
    /// </summary>
    public List<Category> GetAllCategoriesWithSongs()
    {
        return context.Categories
            .Include(c => c.Songs)
            .AsNoTracking()
            .ToList();
    }

    /// <summary>
    /// Gets the total count of songs currently stored in the system.
    /// </summary>
    public int GetTotalSongsCount()
    {
        return context.Songs.Count();
    }

    /// <summary>
    /// Retrieves a unique list of all Tag IDs existing across the entire database.
    /// </summary>
    public List<int> GetAllTagIds()
    {
        return context.Tags
            .AsNoTracking()
            .Select(t => t.TagID)
            .ToList();
    }

    /// <summary>
    /// Retrieves all song-tag frequency records, including eagerly loaded song details.
    /// </summary>
    public List<SongTagFrequency> GetSongTagFrequencies()
    {
        return context.SongTagFrequencies
            .Include(stf => stf.Song)
            .AsNoTracking()
            .ToList();
    }
}