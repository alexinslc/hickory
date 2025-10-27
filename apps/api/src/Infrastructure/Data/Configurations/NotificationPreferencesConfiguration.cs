using Hickory.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hickory.Api.Infrastructure.Data.Configurations;

public class NotificationPreferencesConfiguration : IEntityTypeConfiguration<NotificationPreferences>
{
    public void Configure(EntityTypeBuilder<NotificationPreferences> builder)
    {
        builder.ToTable("NotificationPreferences");
        
        builder.HasKey(np => np.Id);
        
        builder.Property(np => np.UserId)
            .IsRequired();
        
        builder.Property(np => np.WebhookUrl)
            .HasMaxLength(500);
        
        builder.Property(np => np.WebhookSecret)
            .HasMaxLength(200);
        
        builder.Property(np => np.CreatedAt)
            .IsRequired();
        
        builder.Property(np => np.UpdatedAt)
            .IsRequired();
        
        // Index on UserId for fast lookup
        builder.HasIndex(np => np.UserId)
            .IsUnique();
        
        // Relationship with User
        builder.HasOne(np => np.User)
            .WithMany()
            .HasForeignKey(np => np.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
