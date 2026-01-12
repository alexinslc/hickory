using Hickory.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hickory.Api.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for AuditLog - append-only, immutable audit trail
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(a => a.Timestamp)
            .IsRequired();
        
        builder.Property(a => a.Action)
            .IsRequired();
        
        builder.Property(a => a.UserEmail)
            .HasMaxLength(256);
        
        builder.Property(a => a.EntityType)
            .HasMaxLength(100);
        
        builder.Property(a => a.EntityId)
            .HasMaxLength(100);
        
        // JSON columns for flexible storage
        builder.Property(a => a.OldValues)
            .HasColumnType("jsonb");
        
        builder.Property(a => a.NewValues)
            .HasColumnType("jsonb");
        
        builder.Property(a => a.IpAddress)
            .HasMaxLength(45); // IPv6 max length
        
        builder.Property(a => a.UserAgent)
            .HasMaxLength(500);
        
        builder.Property(a => a.Details)
            .HasMaxLength(1000);
        
        // Indexes for common query patterns
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.Action);
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
        
        // Composite index for common admin queries
        builder.HasIndex(a => new { a.Timestamp, a.Action, a.UserId });
    }
}
