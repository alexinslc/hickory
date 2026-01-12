using OtpNet;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Hickory.Api.Features.Auth.TwoFactor;

/// <summary>
/// Implementation of Two-Factor Authentication using TOTP (Time-based One-Time Password)
/// </summary>
public class TwoFactorService : ITwoFactorService
{
    private const int SecretKeyLength = 20; // 160 bits as recommended by RFC 4226
    private const int TotpTimeStep = 30;    // 30-second window
    private const int BackupCodeLength = 8;
    
    /// <inheritdoc />
    public string GenerateSecretKey()
    {
        var key = KeyGeneration.GenerateRandomKey(SecretKeyLength);
        return Base32Encoding.ToString(key);
    }
    
    /// <inheritdoc />
    public string GenerateQrCodeUri(string email, string secretKey, string issuer = "Hickory")
    {
        // Format: otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}&algorithm=SHA1&digits=6&period=30
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedEmail = Uri.EscapeDataString(email);
        var label = $"{encodedIssuer}:{encodedEmail}";
        
        return $"otpauth://totp/{label}?secret={secretKey}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period={TotpTimeStep}";
    }
    
    /// <inheritdoc />
    public bool ValidateCode(string secretKey, string code)
    {
        if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(code))
            return false;
            
        try
        {
            var keyBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(keyBytes, step: TotpTimeStep);
            
            // Allow a window of 1 step before/after current time to account for clock drift
            return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
        }
        catch
        {
            return false;
        }
    }
    
    /// <inheritdoc />
    public List<string> GenerateBackupCodes(int count = 10)
    {
        var codes = new List<string>();
        
        for (int i = 0; i < count; i++)
        {
            // Generate a random 8-character alphanumeric code
            var bytes = RandomNumberGenerator.GetBytes(BackupCodeLength);
            var code = Convert.ToBase64String(bytes)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "")
                .Substring(0, BackupCodeLength)
                .ToUpperInvariant();
            
            // Format as XXXX-XXXX for readability
            codes.Add($"{code.Substring(0, 4)}-{code.Substring(4, 4)}");
        }
        
        return codes;
    }
    
    /// <inheritdoc />
    public bool ValidateBackupCode(string code, string hashedBackupCodes, out string updatedHashedCodes)
    {
        updatedHashedCodes = hashedBackupCodes;
        
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(hashedBackupCodes))
            return false;
            
        try
        {
            // Normalize the input code (remove dashes, uppercase)
            var normalizedCode = code.Replace("-", "").ToUpperInvariant();
            var codeHash = HashCode(normalizedCode);
            
            // Deserialize the stored hashes
            var storedHashes = JsonSerializer.Deserialize<List<string>>(hashedBackupCodes);
            if (storedHashes == null)
                return false;
                
            // Check if the hash exists
            var index = storedHashes.FindIndex(h => h == codeHash);
            if (index == -1)
                return false;
                
            // Remove the used code
            storedHashes.RemoveAt(index);
            updatedHashedCodes = JsonSerializer.Serialize(storedHashes);
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Hashes backup codes for secure storage
    /// </summary>
    public static string HashBackupCodes(List<string> codes)
    {
        var hashes = codes.Select(c => HashCode(c.Replace("-", "").ToUpperInvariant())).ToList();
        return JsonSerializer.Serialize(hashes);
    }
    
    private static string HashCode(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(bytes);
    }
}
