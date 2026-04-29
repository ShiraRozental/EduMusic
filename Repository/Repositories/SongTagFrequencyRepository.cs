using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;
using System.Linq;
using System.Linq.Expressions;

namespace Repository.Repositories
{
    public class SongTagFrequencyRepository(IContext context) : IRepository<SongTagFrequency>
    {
        private readonly IContext ctx = context;
      
        public async Task<SongTagFrequency> AddItem(SongTagFrequency item)
        {
            await ctx.SongTagFrequencies.AddAsync(item);
            await ctx.Save();
            return item;

        }

        public async Task DeleteItem(int id)
        {
            var item = await ctx.SongTagFrequencies.FirstOrDefaultAsync(x => x.FrequencyID == id);
            ctx.SongTagFrequencies.Remove(item);
            await ctx.Save();
        }

        public async Task<List<SongTagFrequency>> GetAll(Expression<Func<SongTagFrequency, bool>> filter = null)
        {
            IQueryable<SongTagFrequency> query = ctx.SongTagFrequencies;
            query = filter != null ? query.Where(filter) : query;
            return await query.ToListAsync();
        }

        public async Task<SongTagFrequency> GetById(int id)
        {
            return await ctx.SongTagFrequencies.FirstOrDefaultAsync(x => x.FrequencyID == id);
        }

        public async Task<SongTagFrequency> UpdateItem(int id, SongTagFrequency songTagFrequency)
        {
            var item = await ctx.SongTagFrequencies.FirstOrDefaultAsync(x => x.FrequencyID == id);
            item.Frequency = songTagFrequency.Frequency;
            item.SongID = songTagFrequency.SongID;
            item.TagID = songTagFrequency.TagID;
            await ctx.Save();
            return item;
        }
    }
}
