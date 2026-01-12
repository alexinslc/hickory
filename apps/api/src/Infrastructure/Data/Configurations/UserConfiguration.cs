using Hickory.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hickory.Api.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the User entity
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Primary key
        builder.HasKey(u => u.Id);
        
        // Properties
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Property(u => u.PasswordHash)
            .HasMaxLength(512);
        
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<int>();
        
        builder.Property(u => u.ExternalProviderId)
            .HasMaxLength(256);
        
        builder.Property(u => u.ExternalProvider)
            .HasMaxLength(50);
        
        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(u => u.CreatedAt)
            .IsRequired();
        
        builder.Property(u => u.LastLoginAt);
        
        // Optimistic concurrency
        builder.Property(u => u.RowVersion)
            .IsRowVersion();
        
        // Two-Factor Authentication
        builder.Property(u => u.TwoFactorEnabled)
            .HasDefaultValue(false);
        
        builder.Property(u => u.TwoFactorSecret)
            .HasMaxLength(512);
        
        builder.Property(u => u.TwoFactorBackupCodes)
            .HasMaxLength(2000); // JSON array of hashed backup codes
        
        builder.Property(u => u.TwoFactorEnabledAt);
        
        // Indexes
        builder.HasIndex(u => u.Email)
            .IsUnique();
        
        builder.HasIndex(u => u.ExternalProviderId);
        
        builder.HasIndex(u => u.Role);
    }
}
