using Common.enums;
using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;
using System.Linq.Expressions;


namespace Repository.Repositories
{
    public class SongRepository( IContext _context) : ISongRepository
    {
        public async Task UpdateSongResultAsync(int songId, string lyrics, int? categoryId, Dictionary<Tag, int> finalTags)
        {
            List<SongTagFrequency> tagFrequencies = finalTags.Select(kv => new SongTagFrequency
            {
                SongID = songId,
                TagID = kv.Key.TagID,
                Frequency = kv.Value
            }).ToList();
            var song = await _context.Songs.FindAsync(songId);
            if (song != null)
            {
                song.RawLyrics = lyrics;
                song.CategoryID = categoryId;
                song.Status = SongStatus.Ready;
                song.TagsFrequencies = tagFrequencies;
                await _context.Save();
            }
        }
        public async Task<Song> AddItem(Song song)
        {
            await _context.Songs.AddAsync(song);
            await _context.Save();
            return song;
        }
        public async Task DeleteItem(int id)
        {
            var deleteItem = await _context.Songs.FirstOrDefaultAsync(x => x.SongID == id);
            if (deleteItem != null)
            { 
                _context.Songs.Remove(deleteItem);
                await _context.Save();
            }
        }

        public async Task<List<Song>> GetAll(Expression<Func<Song, bool>> filter = null)
        {
            IQueryable<Song> query = _context.Songs;
            query = filter != null ? query.Where(filter) : query;
            return await query.ToListAsync();
        }

        public async Task<Song?> GetById(int id)
        {
            return await _context.Songs.FirstOrDefaultAsync(x => x.SongID == id);
        }

        public async Task<Song> UpdateItem(int id, Song song)
        {
            var item = await _context.Songs.FirstOrDefaultAsync(x => x.SongID == id);
            if (item == null) return null;

            item.Title = song.Title;
            item.Artist = song.Artist;
            item.RawLyrics = song.RawLyrics;
            item.FilePath = song.FilePath;
            item.UploaderID = song.UploaderID;
            item.CategoryID = song.CategoryID;

            await _context.Save();
            return item;
        }

        public async Task<Song?> GetSongWithDetails(int id) 
        {             
            return await _context.Songs
                .AsNoTracking() 
                .Include(s => s.Category)
                .Include(s => s.Uploader)
                .Include(s => s.TagsFrequencies) 
                .ThenInclude(stf => stf.Tag)    
                .FirstOrDefaultAsync(s => s.SongID == id);
        }

        public async Task<List<Song>> SearchAsync(string? title, string? artist, int? categoryId, int? tagId)
        {
            // 1. מתחילים משאילתה בסיסית וטוענים את ה-Category (בשביל ה-CategoryName ב-DTO)
            var queryable = _context.Songs
                .Include(s => s.Category)
                .AsQueryable();

            // 2. סינון לפי שם השיר (אם המשתמש הקליד משהו)
            if (!string.IsNullOrWhiteSpace(title))
            {
                queryable = queryable.Where(s => s.Title.Contains(title));
            }

            // 3. סינון לפי שם האומן
            if (!string.IsNullOrWhiteSpace(artist))
            {
                queryable = queryable.Where(s => s.Artist != null && s.Artist.Contains(artist));
            }

            // 4. סינון לפי קטגוריה (אם נבחרה קטגוריה מה-Dropdown)
            if (categoryId.HasValue)
            {
                queryable = queryable.Where(s => s.CategoryID == categoryId.Value);
            }

            // 5. סינון לפי תגית (אם נבחרה תגית מתוך ה-Autocomplete)
            if (tagId.HasValue)
            {
                queryable = queryable.Where(s => s.TagsFrequencies.Any(tf => tf.TagID == tagId.Value));
            }

            // 6. מיון כרונולוגי - השירים החדשים ביותר שהועלו יופיעו ראשונים
            queryable = queryable.OrderByDescending(s => s.UploadDate);

            // 7. ביצוע השאילתה בפועל מול ה-DB והחזרת רשימת הישויות
            return await queryable.ToListAsync();
        }

        public async Task<List<Song>> GetSongsByCategory(int categoryId)
        {
            return await _context.Songs
                .Where(s => s.CategoryID == categoryId)
                .ToListAsync();
        }

        public async Task<Song?> GetByIdWithTagsAsync(int songId)
        {
            return await _context.Songs
                .Include(s => s.TagsFrequencies)
                .FirstOrDefaultAsync(s => s.SongID == songId);
        }

        public async Task UpdateCategoryAsync(int songId, int newCategoryId)
        {
            var song = await _context.Songs.FindAsync(songId);
            if (song == null) return;

            song.CategoryID = newCategoryId;
            await _context.Save();
        }
    }
}
