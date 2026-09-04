namespace Aegis.Api.Models
{
    public class Project
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int OwnerId { get; set; }

        public User? Owner { get; set; }

        public List<ProjectMember> Members { get; set; } = new();

        public List<ProjectTask> Tasks { get; set; } = new();
    }
}  