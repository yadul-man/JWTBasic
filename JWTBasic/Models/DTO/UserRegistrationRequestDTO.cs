using System.ComponentModel.DataAnnotations;

namespace JWTBasic.Models.DTO
{
    public class UserRegistrationRequestDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
