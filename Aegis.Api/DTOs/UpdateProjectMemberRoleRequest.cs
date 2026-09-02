using System.ComponentModel.DataAnnotations;

namespace Aegis.Api.DTOs
{
    public class UpdateProjectMemberRoleRequest
    {
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}