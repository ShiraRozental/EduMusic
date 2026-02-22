
using Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Azure;

namespace EduMusic.DataContext
{
    public class EduMusicContext : DbContext, IContext
    {
        public EduMusicContext(DbContextOptions<EduMusicContext> options) : base(options){}

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<Song> Songs { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<SongTagFrequency> SongTagFrequencies { get; set; }
        public virtual DbSet<Category> Categories { get; set; }

        public async Task Save()
        {
            await SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Song>()
                .HasIndex(s => s.RawLyrics)
                .HasDatabaseName("Index_Lyrics");

            modelBuilder.Entity<Admin>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.ID)
                .IsUnique();

            modelBuilder.Entity<SongTagFrequency>()
                .HasOne(st => st.Song)
                .WithMany(s => s.TagsFrequencies)
                .HasForeignKey(st => st.SongID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}