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

        public DbSet<ProjectMember> ProjectMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Email)
                .IsUnique();

            modelBuilder.Entity<ProjectMember>()
                .HasKey(projectMember => new
                {
                    projectMember.ProjectId,
                    projectMember.UserId
                });

            modelBuilder.Entity<ProjectMember>()
                .HasOne(projectMember => projectMember.Project)
                .WithMany(project => project.Members)
                .HasForeignKey(projectMember => projectMember.ProjectId);

            modelBuilder.Entity<ProjectMember>()
                .HasOne(projectMember => projectMember.User)
                .WithMany(user => user.ProjectMemberships)
                .HasForeignKey(projectMember => projectMember.UserId);
        }
    }
}