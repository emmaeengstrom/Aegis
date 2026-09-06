using System.ComponentModel.DataAnnotations;

namespace Aegis.Api.DTOs
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(254)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(128)]
        public string Password { get; set; } = string.Empty;
    }
} 