namespace Aegis.Api.Models
{
    public class ProjectTask
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty; 

        public string Status { get; set; } = ProjectTaskStatuses.Todo;

        public DateTime CreatedAt { get; set; } =
            DateTime.UtcNow;

        public int ProjectId { get; set; }

        public Project Project { get; set; } = null!;
    }
} 