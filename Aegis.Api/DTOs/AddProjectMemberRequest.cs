using System.ComponentModel.DataAnnotations;

namespace Aegis.Api.DTOs
{
    public class AddProjectMemberRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
} 