using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class TagRepository : IRepository<Tag>
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

        public async Task<List<Tag>> GetAll()
        {
            return await ctx.Tags.ToListAsync();
        }

        public async Task<Tag> GetById(int id)
        {
            return await ctx.Tags.FirstOrDefaultAsync(x => x.TagID == id);
        }

        public async Task<Tag> UpdateItem(int id, Tag tag)
        {
            var item = await ctx.Tags.FirstOrDefaultAsync(x => x.TagID == id);
            item.TagText = tag.TagText;

            await ctx.Save();
            return item;
        }
    }
}
