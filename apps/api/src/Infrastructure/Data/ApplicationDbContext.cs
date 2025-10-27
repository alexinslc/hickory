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
            .Where(e => e.Entity is User && e.State == EntityState.Added);
        
        foreach (var entry in entries)
        {
            if (entry.Entity is User user)
            {
                user.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}
