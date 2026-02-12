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
    public class AdminRepository : IRepository<Admin>
    {

        private readonly IContext ctx;
        public AdminRepository(IContext context)
        {
            ctx = context;
        }

        public async Task<Admin> AddItem(Admin admin)
        {
            ctx.Admins.AddAsync(admin);
            ctx.Save();
            return admin;
        }
        public async Task DeleteItem(int id)
        {
            var deleteItem = await ctx.Admins.FirstOrDefaultAsync(x => x.AdminID == id);
            ctx.Admins.Remove(deleteItem);
            await ctx.Save();
        }

        public async Task<List<Admin>> GetAll()
        {
            return await ctx.Admins.ToListAsync();
        }

        public async Task<Admin> GetById(int id)
        {
            return await ctx.Admins.FirstOrDefaultAsync(x => x.AdminID == id);
        }

        public async Task<Admin> UpdateItem(int id, Admin admin)
        {
            var item = await ctx.Admins.FirstOrDefaultAsync(x => x.AdminID == id);
            item.ImageUrl = admin.ImageUrl;
            item.FullName = admin.FullName;
            item.Email = admin.Email;
            item.Password = admin.Password;
      
            await ctx.Save();
            return item;
        }

    }
}
