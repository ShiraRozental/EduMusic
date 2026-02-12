using Repository.Entities;
using Repository.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Repository.Repositories
{
    public class CategoryRepository : IRepository<Category>
    {
        private readonly IContext ctx;
        public CategoryRepository(IContext context)
        {
            ctx = context;
        }

        public async Task<Category> AddItem(Category category)
        {
            ctx.Categories.AddAsync(category);
            ctx.Save();
            return category;
        }
        public async Task DeleteItem(int id)
        {
            var deleteItem = await ctx.Categories.FirstOrDefaultAsync(x => x.CategoryID == id);
            ctx.Categories.Remove(deleteItem);
            await ctx.Save();
        }

        public async Task<List<Category>> GetAll()
        {
            return await ctx.Categories.ToListAsync();
        }

        public async Task<Category> GetById(int id)
        {
            return await ctx.Categories.FirstOrDefaultAsync(x => x.CategoryID == id);
        }

        public async Task<Category> UpdateItem(int id, Category category)
        {
            var item = await ctx.Categories.FirstOrDefaultAsync(x => x.CategoryID == id);
            item.CategoryName = category.CategoryName;
            await ctx.Save();
            return item;
        }

    }
}
