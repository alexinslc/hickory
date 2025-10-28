using Hickory.Api.Infrastructure.Auth;
using Hickory.Api.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAdminUser(ApplicationDbContext context, IPasswordHasher passwordHasher, ILogger logger)
    {
        try
        {
            // Check if any admin users exist
            var adminExists = await context.Users
                .AnyAsync(u => u.Role == UserRole.Administrator);

            if (!adminExists)
            {
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@hickory.local",
                    PasswordHash = passwordHasher.HashPassword("Admin123!"),
                    FirstName = "Admin",
                    LastName = "User",
                    Role = UserRole.Administrator,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();

                logger.LogInformation("✅ Admin user created successfully");
                logger.LogInformation("   Email: admin@hickory.local");
                logger.LogInformation("   Password: Admin123!");
                logger.LogInformation("   Role: Administrator");
            }
            else
            {
                logger.LogInformation("✅ Admin user already exists - skipping seed");
            }

            // Check if any agent users exist
            var agentExists = await context.Users
                .AnyAsync(u => u.Role == UserRole.Agent);

            if (!agentExists)
            {
                var agentUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "agent@hickory.local",
                    PasswordHash = passwordHasher.HashPassword("Agent123!"),
                    FirstName = "Agent",
                    LastName = "User",
                    Role = UserRole.Agent,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(agentUser);
                await context.SaveChangesAsync();

                logger.LogInformation("✅ Agent user created successfully");
                logger.LogInformation("   Email: agent@hickory.local");
                logger.LogInformation("   Password: Agent123!");
                logger.LogInformation("   Role: Agent");
            }
            else
            {
                logger.LogInformation("✅ Agent user already exists - skipping seed");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error seeding database");
            throw;
        }
    }
}
