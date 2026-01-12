namespace Hickory.Api.Infrastructure.Data.Entities;

/// <summary>
/// Represents a user in the system with authentication details and role assignments
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// User's email address (also used for login)
    /// </summary>
    public required string Email { get; set; }
    
    /// <summary>
    /// Hashed password (null if using SSO only)
    /// </summary>
    public string? PasswordHash { get; set; }
    
    /// <summary>
    /// User's first name
    /// </summary>
    public required string FirstName { get; set; }
    
    /// <summary>
    /// User's last name
    /// </summary>
    public required string LastName { get; set; }
    
    /// <summary>
    /// User's role in the system
    /// </summary>
    public UserRole Role { get; set; }
    
    /// <summary>
    /// External identity provider ID (for OAuth/OIDC)
    /// </summary>
    public string? ExternalProviderId { get; set; }
    
    /// <summary>
    /// Provider name (e.g., "Google", "AzureAD")
    /// </summary>
    public string? ExternalProvider { get; set; }
    
    /// <summary>
    /// Whether the account is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Account creation timestamp (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Last successful login timestamp (UTC)
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// Optimistic concurrency token
    /// </summary>
    public byte[]? RowVersion { get; set; }
    
    // Two-Factor Authentication (2FA)
    
    /// <summary>
    /// Whether 2FA is enabled for this user
    /// </summary>
    public bool TwoFactorEnabled { get; set; }
    
    /// <summary>
    /// Encrypted TOTP secret key (Base32 encoded)
    /// </summary>
    public string? TwoFactorSecret { get; set; }
    
    /// <summary>
    /// Hashed backup codes for 2FA recovery (JSON array)
    /// </summary>
    public string? TwoFactorBackupCodes { get; set; }
    
    /// <summary>
    /// When 2FA was enabled
    /// </summary>
    public DateTime? TwoFactorEnabledAt { get; set; }
}
