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

            modelBuilder.Entity<User>(entity =>
            {
                entity
                    .HasIndex(user => user.Email)
                    .IsUnique();

                entity
                    .Property(user => user.Email)
                    .HasMaxLength(254)
                    .IsRequired();

                entity
                    .Property(user => user.PasswordHash)
                    .HasMaxLength(512)
                    .IsRequired();
            }); 

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
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProjectTask>()
                .HasOne(task => task.Project)
                .WithMany(project => project.Tasks)
                .HasForeignKey(task => task.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Project>(entity =>
            {
                entity
                    .Property(project => project.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity
                    .Property(project => project.Description)
                    .HasMaxLength(500)
                    .IsRequired();
            });

            modelBuilder.Entity<ProjectMember>(entity =>
            {
                entity
                    .Property(projectMember => projectMember.Role)
                    .HasMaxLength(32)
                    .IsRequired();
            });

            modelBuilder.Entity<ProjectTask>(entity =>
            {
                entity
                    .Property(task => task.Title)
                    .HasMaxLength(150)
                    .IsRequired();

                entity
                    .Property(task => task.Description)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity
                    .Property(task => task.Status)
                    .HasMaxLength(32)
                    .IsRequired();
            }); 
        }
    }
} 