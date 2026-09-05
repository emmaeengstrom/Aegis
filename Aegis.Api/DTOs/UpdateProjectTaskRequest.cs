using System.ComponentModel.DataAnnotations;

namespace Aegis.Api.DTOs
{
    public class UpdateProjectTaskRequest
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty;
    }
} 