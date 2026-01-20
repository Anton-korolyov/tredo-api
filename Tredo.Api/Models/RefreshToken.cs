using System.ComponentModel.DataAnnotations;

namespace Tredo.Api.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string TokenHash { get; set; } = "";

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresUtc { get; set; }

        public DateTime? RevokedUtc { get; set; }

        public bool IsActive => RevokedUtc == null && DateTime.UtcNow < ExpiresUtc;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
