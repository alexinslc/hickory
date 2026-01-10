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
        
        // Full-text search vector (PostgreSQL tsvector)
        builder.Property(t => t.SearchVector)
            .HasColumnType("tsvector")
            .HasComputedColumnSql(
                "to_tsvector('english', coalesce(\"TicketNumber\", '') || ' ' || coalesce(\"Title\", '') || ' ' || coalesce(\"Description\", ''))",
                stored: true);
        
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
        
        // Many-to-many relationship with Tags through TicketTag
        builder.HasMany(t => t.TicketTags)
            .WithOne(tt => tt.Ticket)
            .HasForeignKey(tt => tt.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes for performance
        builder.HasIndex(t => t.SubmitterId);
        builder.HasIndex(t => t.AssignedToId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Priority);
        builder.HasIndex(t => t.CategoryId);
        builder.HasIndex(t => t.CreatedAt);
        builder.HasIndex(t => t.UpdatedAt);
        
        // Composite indexes for common query patterns
        // Agent queue queries (status + priority with created date for sorting)
        builder.HasIndex(t => new { t.Status, t.Priority, t.CreatedAt })
            .HasDatabaseName("IX_Tickets_Status_Priority_CreatedAt");
        
        // User's tickets by submitter (submitter + status for filtering)
        builder.HasIndex(t => new { t.SubmitterId, t.Status, t.CreatedAt })
            .HasDatabaseName("IX_Tickets_SubmitterId_Status_CreatedAt");
        
        // Assigned tickets (assignee + status for agent workload)
        builder.HasIndex(t => new { t.AssignedToId, t.Status, t.Priority })
            .HasDatabaseName("IX_Tickets_AssignedToId_Status_Priority")
            .HasFilter("\"AssignedToId\" IS NOT NULL");
        
        // Category-based queries
        builder.HasIndex(t => new { t.CategoryId, t.Status, t.CreatedAt })
            .HasDatabaseName("IX_Tickets_CategoryId_Status_CreatedAt")
            .HasFilter("\"CategoryId\" IS NOT NULL");
        
        // Recent activity queries (updated date with status)
        builder.HasIndex(t => new { t.UpdatedAt, t.Status })
            .HasDatabaseName("IX_Tickets_UpdatedAt_Status");
        
        // GIN index for full-text search
        builder.HasIndex(t => t.SearchVector)
            .HasMethod("GIN");
    }
}
