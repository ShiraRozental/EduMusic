using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;


namespace Repository.Repositories
{
    public class SongRepository( IContext _context) : ISongRepository
    {

        public async Task<Song> AddItem(Song song)
        {
            _context.Songs.AddAsync(song);
            _context.Save();
            return song;
        }
        public async Task DeleteItem(int id)
        {
            var deleteItem = await _context.Songs.FirstOrDefaultAsync(x => x.SongID == id);
            _context.Songs.Remove(deleteItem);
            await _context.Save();
        }

        public async Task<List<Song>> GetAll()
        {
            return await _context.Songs.ToListAsync();
        }

        public async Task<Song> GetById(int id)
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

        public async Task<List<Song>> SearchSongs(string query)
        {
            return await _context.Songs
                .Where(s => s.Title.Contains(query) || s.Artist.Contains(query) || s.RawLyrics.Contains(query) || s.Category.CategoryName.Contains(query))
                .ToListAsync();
        }

        public async Task<List<Song>> GetSongsByCategory(int categoryId)
        {
            return await _context.Songs
                .Where(s => s.CategoryID == categoryId)
                .ToListAsync();
        }
    }
}
