using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Hickory.Api.Tests.TestUtilities;

/// <summary>
/// Factory for creating in-memory DbContext instances for unit testing
/// </summary>
public static class TestDbContextFactory
{
    public static ApplicationDbContext CreateInMemoryDbContext(string databaseName = "")
    {
        if (string.IsNullOrEmpty(databaseName))
        {
            databaseName = Guid.NewGuid().ToString();
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .ConfigureWarnings(warnings =>
            {
                // Suppress warnings about InMemory not supporting transactions
                warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning);
            })
            .EnableSensitiveDataLogging()
            .Options;

        var context = new TestApplicationDbContext(options);

        // Ensure the database is created (this sets up the model)
        context.Database.EnsureCreated();

        return context;
    }
}

/// <summary>
/// Test-specific DbContext that ignores PostgreSQL-specific properties
/// </summary>
internal class TestApplicationDbContext : ApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ignore PostgreSQL-specific properties that InMemory database doesn't support
        modelBuilder.Entity<KnowledgeArticle>()
            .Ignore(a => a.SearchVector);

        modelBuilder.Entity<Ticket>()
            .Ignore(t => t.SearchVector);
    }
}
