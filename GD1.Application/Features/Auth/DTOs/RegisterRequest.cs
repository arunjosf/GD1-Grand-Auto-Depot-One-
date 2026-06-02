using System.ComponentModel.DataAnnotations;

namespace GD1.Application.Features.Auth.DTOs
{
    public class RegisterRequest
    {
        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Required]
        public string ConfirmPassword { get; set; } = null!;
    }
}
