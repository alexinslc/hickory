using Hickory.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hickory.Api.Infrastructure.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");
        
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(50);
        
        // Case-insensitive unique index on name
        builder.HasIndex(t => t.Name)
            .IsUnique();
        
        builder.Property(t => t.Color)
            .HasMaxLength(7); // Hex color format: #RRGGBB
        
        builder.Property(t => t.CreatedAt)
            .IsRequired();
    }
}
