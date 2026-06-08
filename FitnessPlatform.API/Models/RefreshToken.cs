using System;

namespace FitnessPlatform.API.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public required string Token { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }

        public string? ReplacedByToken { get; set; }

        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;

        public required string UserId { get; set; }

        public AppUser User { get; set; } = null!;
    }
}
