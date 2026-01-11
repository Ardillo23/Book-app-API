using Microsoft.EntityFrameworkCore;
using VistaTiBooks.Api.Models;

namespace VistaTiBooks.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Favorite> Favorites => Set<Favorite>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.ExternalId })
                .IsUnique();
        }
    }
}
