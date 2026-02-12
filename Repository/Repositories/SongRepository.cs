using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;


namespace Repository.Repositories
{
    public class SongRepository : IRepository<Song>
    {
        private readonly IContext ctx;
        public SongRepository(IContext context)
        {
            ctx = context;
        }

        public async Task<Song> AddItem(Song song)
        {
            ctx.Songs.AddAsync(song);
            ctx.Save();
            return song;
        }
        public async Task DeleteItem(int id)
        {
            var deleteItem = await ctx.Songs.FirstOrDefaultAsync(x => x.SongID == id);
            ctx.Songs.Remove(deleteItem);
            await ctx.Save();
        }

        public async Task<List<Song>> GetAll()
        {
            return await ctx.Songs.ToListAsync();
        }

        public async Task<Song> GetById(int id)
        {
            return await ctx.Songs.FirstOrDefaultAsync(x => x.SongID == id);
        }

        public async Task<Song> UpdateItem(int id, Song song)
        {
            var item = await ctx.Songs.FirstOrDefaultAsync(x => x.SongID == id);
            if (item == null) return null;

            item.Title = song.Title;
            item.Artist = song.Artist;
            item.RawLyrics = song.RawLyrics;
            item.FilePath = song.FilePath;
            item.UploaderID = song.UploaderID;
            item.CategoryID = song.CategoryID;

            await ctx.Save();
            return item;
        }
    }
}
