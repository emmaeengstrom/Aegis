namespace Aegis.Api.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int ProjectId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string Details { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;

        public Project Project { get; set; } = null!;
    }
} 