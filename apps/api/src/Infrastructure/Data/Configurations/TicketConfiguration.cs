using Hickory.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hickory.Api.Infrastructure.Data.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");
        
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.TicketNumber)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.HasIndex(t => t.TicketNumber)
            .IsUnique();
        
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(10000);
        
        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>() // Store as string in DB
            .HasMaxLength(20);
        
        builder.Property(t => t.Priority)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
        
        builder.Property(t => t.ResolutionNotes)
            .HasMaxLength(5000);
        
        builder.Property(t => t.CreatedAt)
            .IsRequired();
        
        builder.Property(t => t.UpdatedAt)
            .IsRequired();
        
        // Optimistic Concurrency
        builder.Property(t => t.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate(); // Required for PostgreSQL to auto-update
        
        // Relationships
        builder.HasOne(t => t.Submitter)
            .WithMany()
            .HasForeignKey(t => t.SubmitterId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(t => t.AssignedTo)
            .WithMany()
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
        
        builder.HasMany(t => t.Comments)
            .WithOne(c => c.Ticket)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(t => t.Attachments)
            .WithOne(a => a.Ticket)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes for performance
        builder.HasIndex(t => t.SubmitterId);
        builder.HasIndex(t => t.AssignedToId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Priority);
        builder.HasIndex(t => t.CreatedAt);
        
        // Composite index for agent queue queries
        builder.HasIndex(t => new { t.Status, t.Priority, t.CreatedAt });
    }
}
