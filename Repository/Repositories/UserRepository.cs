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
    public class UserRepository(IContext _context) : IUserRepository
    {
        public async Task<User> AddItem(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.Save();
            return user;
        }

        public async Task DeleteItem(int id)
        {
            var item = await _context.Users.FirstOrDefaultAsync(x => x.UserID == id);
            if (item == null) return;
            _context.Users.Remove(item);
            await _context.Save();
        }

        public async Task<List<User>> GetAll()
        {
            return await _context.Users.ToListAsync();
        }
       
        public async Task<User> GetById(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.UserID == id);
        }

        public async Task<User> UpdateItem(int id, User user)
        {
            var item = await _context.Users.FirstOrDefaultAsync(x => x.UserID == id);
            item.MyTeacherID = user.MyTeacherID;
            item.ID = user.ID;

            await _context.Save();
            return item;

        }

        public async Task<Admin?> GetAdminForUser(int userId)
        {
            var user = await _context.Users
                .Include(u => u.MyTeacher) 
                .FirstOrDefaultAsync(u => u.UserID == userId);

            return user?.MyTeacher;
        }
    }
}

