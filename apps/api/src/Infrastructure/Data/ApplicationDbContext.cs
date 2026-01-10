using Hickory.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Infrastructure.Data;

/// <summary>
/// Main database context for the Hickory Help Desk application
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    /// <summary>
    /// Users in the system
    /// </summary>
    public DbSet<User> Users => Set<User>();
    
    /// <summary>
    /// Support tickets
    /// </summary>
    public DbSet<Ticket> Tickets => Set<Ticket>();
    
    /// <summary>
    /// Comments on tickets
    /// </summary>
    public DbSet<Comment> Comments => Set<Comment>();
    
    /// <summary>
    /// File attachments on tickets
    /// </summary>
    public DbSet<Attachment> Attachments => Set<Attachment>();
    
    /// <summary>
    /// Categories for organizing tickets
    /// </summary>
    public DbSet<Category> Categories => Set<Category>();
    
    /// <summary>
    /// Tags for labeling tickets
    /// </summary>
    public DbSet<Tag> Tags => Set<Tag>();
    
    /// <summary>
    /// Many-to-many join table for tickets and tags
    /// </summary>
    public DbSet<TicketTag> TicketTags => Set<TicketTag>();
    
    /// <summary>
    /// User notification preferences
    /// </summary>
    public DbSet<NotificationPreferences> NotificationPreferences => Set<NotificationPreferences>();
    
    /// <summary>
    /// Knowledge base articles
    /// </summary>
    public DbSet<KnowledgeArticle> KnowledgeArticles => Set<KnowledgeArticle>();
    
    /// <summary>
    /// Refresh tokens for JWT authentication
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all entity configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
    
    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void SetTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
        
        var now = DateTime.UtcNow;
        
        foreach (var entry in entries)
        {
            switch (entry.Entity)
            {
                case User user when entry.State == EntityState.Added:
                    user.CreatedAt = now;
                    break;
                    
                case Ticket ticket:
                    if (entry.State == EntityState.Added)
                    {
                        ticket.CreatedAt = now;
                        ticket.UpdatedAt = now;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        ticket.UpdatedAt = now;
                    }
                    break;
                    
                case Comment comment:
                    if (entry.State == EntityState.Added)
                    {
                        comment.CreatedAt = now;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        comment.UpdatedAt = now;
                    }
                    break;
                    
                case Attachment attachment when entry.State == EntityState.Added:
                    attachment.UploadedAt = now;
                    break;
                    
                case Category category:
                    if (entry.State == EntityState.Added)
                    {
                        category.CreatedAt = now;
                        category.UpdatedAt = now;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        category.UpdatedAt = now;
                    }
                    break;
                    
                case Tag tag when entry.State == EntityState.Added:
                    tag.CreatedAt = now;
                    break;
                    
                case TicketTag ticketTag when entry.State == EntityState.Added:
                    ticketTag.AddedAt = now;
                    break;
                    
                case NotificationPreferences prefs:
                    if (entry.State == EntityState.Added)
                    {
                        prefs.CreatedAt = now;
                        prefs.UpdatedAt = now;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        prefs.UpdatedAt = now;
                        // Prevent CreatedAt from being modified
                        entry.Property(nameof(prefs.CreatedAt)).IsModified = false;
                    }
                    break;
                    
                case KnowledgeArticle article:
                    if (entry.State == EntityState.Added)
                    {
                        article.CreatedAt = now;
                        article.UpdatedAt = now;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        article.UpdatedAt = now;
                        // Prevent CreatedAt from being modified
                        entry.Property(nameof(article.CreatedAt)).IsModified = false;
                    }
                    break;
                    
                case RefreshToken refreshToken when entry.State == EntityState.Added:
                    refreshToken.CreatedAt = now;
                    break;
            }
        }
    }
}
