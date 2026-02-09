using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using System.Collections.Generic;

namespace Repository.Interfaces
{
    public interface IContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<SongTagFrequency> SongTagFrequencies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public Task Save();
    }
}
