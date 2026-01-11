namespace Hickory.Api.Infrastructure.Data.Entities;

/// <summary>
/// Represents a refresh token for JWT authentication
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required string Token { get; set; }
    public required DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? RevokedReason { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;
    
    // Navigation property
    public User? User { get; set; }
}
