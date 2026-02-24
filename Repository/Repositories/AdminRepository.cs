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
    public class AdminRepository(IContext _context) : IAdminRepository
    {

        public async Task<Admin> AddItem(Admin admin)
        {
            _context.Admins.AddAsync(admin);
            _context.Save();
            return admin;
        }
        public async Task DeleteItem(int id)
        {
            var deleteItem = await _context.Admins.FirstOrDefaultAsync(x => x.AdminID == id);
            _context.Admins.Remove(deleteItem);
            await _context.Save();
        }

        public async Task<List<Admin>> GetAll()
        {
            return await _context.Admins.ToListAsync();
        }

        public async Task<Admin> GetById(int id)
        {
            return await _context.Admins.FirstOrDefaultAsync(x => x.AdminID == id);
        }

        public async Task<Admin> UpdateItem(int id, Admin admin)
        {
            var item = await _context.Admins.FirstOrDefaultAsync(x => x.AdminID == id);
            item.ImageUrl = admin.ImageUrl;
            item.FullName = admin.FullName;
            item.Email = admin.Email;
            item.Password = admin.Password;
      
            await _context.Save();
            return item;
        }

        public async Task<Admin?> GetAdminByEmail(string email)
        {
            return await _context.Admins.FirstOrDefaultAsync(u => u.Email == email);
        }

    }
}
