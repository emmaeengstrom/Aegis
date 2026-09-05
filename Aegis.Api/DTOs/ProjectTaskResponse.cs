namespace Aegis.Api.DTOs
{
    public class ProjectTaskResponse
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public int ProjectId { get; set; }
    }
} 