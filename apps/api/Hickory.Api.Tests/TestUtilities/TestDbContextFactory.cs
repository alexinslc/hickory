using Hickory.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}
