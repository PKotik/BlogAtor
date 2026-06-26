using Microsoft.EntityFrameworkCore;
using BlogAtor.Server.Models;

namespace BlogAtor.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<RedditPost> RedditPosts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RedditPost>()
                .HasIndex(p => p.PostId)
                .IsUnique();
        }
    }
}