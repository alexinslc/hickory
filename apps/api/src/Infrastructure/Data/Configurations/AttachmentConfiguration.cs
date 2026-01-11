using Hickory.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hickory.Api.Infrastructure.Data.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");
        
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(a => a.ContentType)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(a => a.FileSizeBytes)
            .IsRequired();
        
        builder.Property(a => a.StoragePath)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(a => a.UploadedAt)
            .IsRequired();
        
        // Relationships
        builder.HasOne(a => a.Ticket)
            .WithMany(t => t.Attachments)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(a => a.Comment)
            .WithMany(c => c.Attachments)
            .HasForeignKey(a => a.CommentId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
        
        builder.HasOne(a => a.UploadedBy)
            .WithMany()
            .HasForeignKey(a => a.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(a => a.TicketId);
        builder.HasIndex(a => a.CommentId);
        builder.HasIndex(a => a.UploadedAt);
        builder.HasIndex(a => a.UploadedById);
    }
}
