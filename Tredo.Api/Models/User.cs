using System.ComponentModel.DataAnnotations;

namespace Tredo.Api.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}
