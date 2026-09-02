namespace Aegis.Api.DTOs
{
    public class AuditLogResponse
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string UserEmail { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string Details { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}