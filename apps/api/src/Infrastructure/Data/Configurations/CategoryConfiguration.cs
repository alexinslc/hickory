using Hickory.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hickory.Api.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasIndex(c => c.Name)
            .IsUnique();
        
        builder.Property(c => c.Description)
            .HasMaxLength(500);
        
        builder.Property(c => c.DisplayOrder)
            .IsRequired();
        
        builder.Property(c => c.Color)
            .HasMaxLength(7); // Hex color format: #RRGGBB
        
        builder.Property(c => c.CreatedAt)
            .IsRequired();
        
        builder.Property(c => c.UpdatedAt)
            .IsRequired();
        
        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        // Relationships
        builder.HasMany(c => c.Tickets)
            .WithOne(t => t.Category)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
        
        // Indexes for performance
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.DisplayOrder);
    }
}
