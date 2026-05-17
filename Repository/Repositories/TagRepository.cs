using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    // Updated to implement ITagRepository to allow specific batch processing methods
    public class TagRepository : ITagRepository
    {
        private readonly IContext ctx;

        public TagRepository(IContext context)
        {
            ctx = context;
        }

        public async Task<Tag> AddItem(Tag tag)
        {
            await ctx.Tags.AddAsync(tag);
            await ctx.Save();
            return tag;
        }

        public async Task DeleteItem(int id)
        {
            var item = await ctx.Tags.FirstOrDefaultAsync(x => x.TagID == id);
            if (item == null) return;
            ctx.Tags.Remove(item);
            await ctx.Save();
        }

        public async Task<List<Tag>> GetAll(Expression<Func<Tag, bool>> filter = null)
        {
            IQueryable<Tag> query = ctx.Tags;
            query = filter != null ? query.Where(filter) : query;
            return await query.ToListAsync();
        }

        public async Task<Tag?> GetById(int id)
        {
            return await ctx.Tags.FirstOrDefaultAsync(x => x.TagID == id);
        }

        public async Task<Tag> UpdateItem(int id, Tag tag)
        {
            var item = await ctx.Tags.FirstOrDefaultAsync(x => x.TagID == id);
            if (item != null)
            {
                item.TagText = tag.TagText;
                await ctx.Save();
            }
            return item;
        }


        /// <summary>
        /// Adds a tag to the memory context tracker without executing an immediate database save.
        /// </summary>
        public async Task AddWithoutSave(Tag tag)
        {
            await ctx.Tags.AddAsync(tag);
        }

        /// <summary>
        /// Commits all currently cached in-memory changes directly to the database in a single roundtrip.
        /// </summary>
        public async Task SaveAsync()
        {
            await ctx.Save();
        }
    }
}