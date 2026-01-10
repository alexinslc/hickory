using Hickory.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hickory.Api.Infrastructure.Data.Configurations;

public class KnowledgeArticleConfiguration : IEntityTypeConfiguration<KnowledgeArticle>
{
    public void Configure(EntityTypeBuilder<KnowledgeArticle> builder)
    {
        builder.ToTable("KnowledgeArticles");
        
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(a => a.Content)
            .IsRequired()
            .HasMaxLength(50000); // 50KB max for article content
        
        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
        
        builder.Property(a => a.ViewCount)
            .HasDefaultValue(0);
        
        builder.Property(a => a.HelpfulCount)
            .HasDefaultValue(0);
        
        builder.Property(a => a.NotHelpfulCount)
            .HasDefaultValue(0);
        
        builder.Property(a => a.CreatedAt)
            .IsRequired();
        
        builder.Property(a => a.UpdatedAt)
            .IsRequired();
        
        builder.Property(a => a.PublishedAt)
            .IsRequired(false);
        
        // Search vector for PostgreSQL full-text search
        // Computed column: setweight(to_tsvector('english', title), 'A') || setweight(to_tsvector('english', content), 'B')
        // Title is weighted higher ('A') for better search relevance
        builder.Property(a => a.SearchVector)
            .HasColumnType("tsvector")
            .HasComputedColumnSql("setweight(to_tsvector('english', coalesce(\"Title\", '')), 'A') || setweight(to_tsvector('english', coalesce(\"Content\", '')), 'B')", stored: true)
            .IsRequired();
        
        // Relationships
        builder.HasOne(a => a.Author)
            .WithMany()
            .HasForeignKey(a => a.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(a => a.LastUpdatedBy)
            .WithMany()
            .HasForeignKey(a => a.LastUpdatedById)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
        
        builder.HasOne(a => a.Category)
            .WithMany()
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
        
        // Many-to-many with Tags (using existing Tag entity)
        builder.HasMany(a => a.Tags)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "ArticleTags",
                j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId").OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<KnowledgeArticle>().WithMany().HasForeignKey("ArticleId").OnDelete(DeleteBehavior.Cascade));
        
        // Indexes
        builder.HasIndex(a => a.Status)
            .HasDatabaseName("IX_KnowledgeArticles_Status");
        
        builder.HasIndex(a => a.CategoryId)
            .HasDatabaseName("IX_KnowledgeArticles_CategoryId");
        
        builder.HasIndex(a => a.CreatedAt)
            .HasDatabaseName("IX_KnowledgeArticles_CreatedAt");
        
        builder.HasIndex(a => a.PublishedAt)
            .HasDatabaseName("IX_KnowledgeArticles_PublishedAt");
        
        // GIN index for full-text search (created via migration)
        builder.HasIndex(a => a.SearchVector)
            .HasDatabaseName("IX_KnowledgeArticles_SearchVector")
            .HasMethod("gin");
    }
}
