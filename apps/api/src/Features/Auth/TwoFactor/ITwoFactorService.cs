namespace Hickory.Api.Features.Auth.TwoFactor;

/// <summary>
/// Service for Two-Factor Authentication (2FA) operations using TOTP
/// </summary>
public interface ITwoFactorService
{
    /// <summary>
    /// Generates a new secret key for TOTP
    /// </summary>
    string GenerateSecretKey();
    
    /// <summary>
    /// Generates a QR code URI for authenticator apps
    /// </summary>
    string GenerateQrCodeUri(string email, string secretKey, string issuer = "Hickory");
    
    /// <summary>
    /// Validates a TOTP code against a secret key
    /// </summary>
    bool ValidateCode(string secretKey, string code);
    
    /// <summary>
    /// Generates backup codes for recovery
    /// </summary>
    List<string> GenerateBackupCodes(int count = 10);
    
    /// <summary>
    /// Validates a backup code (one-time use)
    /// </summary>
    bool ValidateBackupCode(string code, string hashedBackupCodes, out string updatedHashedCodes);
}
