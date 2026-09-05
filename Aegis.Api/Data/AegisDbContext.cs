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

        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<ProjectTask> ProjectTasks { get; set; }

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

            modelBuilder.Entity<AuditLog>()
                .HasOne(auditLog => auditLog.User)
                .WithMany()
                .HasForeignKey(auditLog => auditLog.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AuditLog>()
                .HasOne(auditLog => auditLog.Project)
                .WithMany()
                .HasForeignKey(auditLog => auditLog.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectTask>()
                .HasOne(task => task.Project)
                .WithMany(project => project.Tasks)
                .HasForeignKey(task => task.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 