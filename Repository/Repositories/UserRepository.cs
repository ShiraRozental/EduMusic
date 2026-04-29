using Repository.Interfaces;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Repository.Repositories
{
    public class UserRepository(IContext context) : IUserRepository
    {
        private readonly IContext _context = context;
        public async Task<User> AddItem(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.Save();
            return user;
        }

        public async Task AddUsersRangeAsync(IEnumerable<User> users)
        {
            await _context.Users.AddRangeAsync(users);
            await _context.Save();
        }

        public async Task DeleteItem(int id)
        {
            var item = await _context.Users.FirstOrDefaultAsync(x => x.UserID == id);
            if (item == null) return;
            _context.Users.Remove(item);
            await _context.Save();
        }

        public async Task<List<User>> GetAll(Expression<Func<User, bool>> filter = null)
        {
            IQueryable<User> query = _context.Users;
            query = filter != null ? query.Where(filter) : query;
            return await query.ToListAsync();
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
            item.FullNameUser = user.FullNameUser;

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

