using Hickory.Api.Infrastructure.Data.Entities;

namespace Hickory.Api.Tests.TestUtilities;

/// <summary>
/// Builder for creating test data entities
/// </summary>
public static class TestDataBuilder
{
    public static User CreateTestUser(
        string email = "test@example.com",
        string firstName = "Test",
        string lastName = "User",
        UserRole role = UserRole.EndUser,
        bool isActive = true,
        string? passwordHash = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            IsActive = isActive,
            PasswordHash = passwordHash ?? "$2a$11$TestHashedPasswordForTesting",
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = null
        };
    }

    public static Ticket CreateTestTicket(
        Guid? submitterId = null,
        string ticketNumber = "TKT-00001",
        string title = "Test Ticket",
        string description = "Test Description",
        TicketStatus status = TicketStatus.Open,
        TicketPriority priority = TicketPriority.Medium,
        Guid? assignedToId = null,
        Guid? categoryId = null)
    {
        return new Ticket
        {
            Id = Guid.NewGuid(),
            TicketNumber = ticketNumber,
            Title = title,
            Description = description,
            Status = status,
            Priority = priority,
            SubmitterId = submitterId ?? Guid.NewGuid(),
            AssignedToId = assignedToId,
            CategoryId = categoryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static Category CreateTestCategory(
        string name = "Test Category",
        string? description = null)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Comment CreateTestComment(
        Guid ticketId,
        Guid authorId,
        string content = "Test comment",
        bool isInternal = false)
    {
        return new Comment
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            AuthorId = authorId,
            Content = content,
            IsInternal = isInternal,
            CreatedAt = DateTime.UtcNow
        };
    }
}
