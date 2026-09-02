using Aegis.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Aegis.Api.Data
{
    public class AegisDbContext : DbContext
    {
        public AegisDbContext(DbContextOptions<AegisDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Email)
                .IsUnique();
        }
    }
}