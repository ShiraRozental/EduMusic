using Repository.Interfaces;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public class UserRepository : IRepository<User>
    {

        private readonly IContext ctx;
        public UserRepository(IContext context)
        {
            ctx = context;

        }
        public async Task<User> AddItem(User user)
        {
            await ctx.Users.AddAsync(user);
            await ctx.Save();
            return user;

        }

        public async Task DeleteItem(int id)
        {
            var item = await ctx.Users.FirstOrDefaultAsync(x => x.UserID == id);
            if (item == null) return;
            ctx.Users.Remove(item);
            await ctx.Save();
        }

        public async Task<List<User>> GetAll()
        {
            return await ctx.Users.ToListAsync();
        }

        public async Task<User> GetById(int id)
        {
            return await ctx.Users.FirstOrDefaultAsync(x => x.UserID == id);
        }

        public async Task<User> UpdateItem(int id, User user)
        {
            var item = await ctx.Users.FirstOrDefaultAsync(x => x.UserID == id);
            item.MyTeacherID = user.MyTeacherID;
            item.ID = user.ID;

            await ctx.Save();
            return item;

        }
    }
}

