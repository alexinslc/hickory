namespace Hickory.Api.Features.Auth.TwoFactor;

/// <summary>
/// Response when initiating 2FA setup
/// </summary>
public record TwoFactorSetupResponse
{
    /// <summary>
    /// Secret key in Base32 format for manual entry
    /// </summary>
    public required string Secret { get; init; }
    
    /// <summary>
    /// QR code URI for authenticator apps (otpauth://)
    /// </summary>
    public required string QrCodeUri { get; init; }
}

/// <summary>
/// Request to enable 2FA after setup
/// </summary>
public record TwoFactorEnableRequest
{
    /// <summary>
    /// The TOTP code from the authenticator app to verify setup
    /// </summary>
    public required string Code { get; init; }
}

/// <summary>
/// Response after enabling 2FA with backup codes
/// </summary>
public record TwoFactorEnableResponse
{
    /// <summary>
    /// Whether 2FA was successfully enabled
    /// </summary>
    public bool Enabled { get; init; }
    
    /// <summary>
    /// Backup codes for recovery (show once, user must save these)
    /// </summary>
    public required List<string> BackupCodes { get; init; }
}

/// <summary>
/// Request to disable 2FA
/// </summary>
public record TwoFactorDisableRequest
{
    /// <summary>
    /// Current password for confirmation
    /// </summary>
    public required string Password { get; init; }
}

/// <summary>
/// Request to verify 2FA during login
/// </summary>
public record TwoFactorVerifyRequest
{
    /// <summary>
    /// The user ID from the initial login response
    /// </summary>
    public required Guid UserId { get; init; }
    
    /// <summary>
    /// The TOTP code or backup code
    /// </summary>
    public required string Code { get; init; }
    
    /// <summary>
    /// Whether this is a backup code (optional, auto-detected if not set)
    /// </summary>
    public bool? IsBackupCode { get; init; }
}

/// <summary>
/// Response from initial login when 2FA is required
/// </summary>
public record TwoFactorRequiredResponse
{
    /// <summary>
    /// Indicates 2FA is required to complete login
    /// </summary>
    public bool Requires2FA { get; init; } = true;
    
    /// <summary>
    /// User ID needed for the 2FA verification step
    /// </summary>
    public required Guid UserId { get; init; }
    
    /// <summary>
    /// User's email for display
    /// </summary>
    public required string Email { get; init; }
}

/// <summary>
/// 2FA status response for user profile
/// </summary>
public record TwoFactorStatusResponse
{
    /// <summary>
    /// Whether 2FA is enabled
    /// </summary>
    public bool Enabled { get; init; }
    
    /// <summary>
    /// When 2FA was enabled (null if not enabled)
    /// </summary>
    public DateTime? EnabledAt { get; init; }
    
    /// <summary>
    /// Number of unused backup codes remaining
    /// </summary>
    public int BackupCodesRemaining { get; init; }
}

/// <summary>
/// Response when regenerating backup codes
/// </summary>
public record TwoFactorRegenerateCodesResponse
{
    /// <summary>
    /// New backup codes (show once, user must save these)
    /// </summary>
    public required List<string> BackupCodes { get; init; }
}
