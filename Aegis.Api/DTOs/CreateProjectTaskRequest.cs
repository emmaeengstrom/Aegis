using System.ComponentModel.DataAnnotations;

namespace Aegis.Api.DTOs
{
    public class CreateProjectTaskRequest
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
    }
} 