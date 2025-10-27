using Hickory.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hickory.Api.Infrastructure.Data.Configurations;

public class TicketTagConfiguration : IEntityTypeConfiguration<TicketTag>
{
    public void Configure(EntityTypeBuilder<TicketTag> builder)
    {
        builder.ToTable("TicketTags");
        
        // Composite primary key
        builder.HasKey(tt => new { tt.TicketId, tt.TagId });
        
        builder.Property(tt => tt.AddedAt)
            .IsRequired();
        
        // Relationships configured in TicketConfiguration and TagConfiguration
        builder.HasOne(tt => tt.Tag)
            .WithMany(t => t.TicketTags)
            .HasForeignKey(tt => tt.TagId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes for performance
        builder.HasIndex(tt => tt.TagId);
        builder.HasIndex(tt => tt.AddedAt);
    }
}
