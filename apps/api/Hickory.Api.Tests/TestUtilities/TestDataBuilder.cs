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

    public static KnowledgeArticle CreateTestKnowledgeArticle(
        Guid? authorId = null,
        string title = "Test Article",
        string content = "Test Content",
        ArticleStatus status = ArticleStatus.Published,
        Guid? categoryId = null,
        int viewCount = 0,
        int helpfulCount = 0,
        int notHelpfulCount = 0,
        DateTime? publishedAt = null)
    {
        return new KnowledgeArticle
        {
            Id = Guid.NewGuid(),
            Title = title,
            Content = content,
            Status = status,
            CategoryId = categoryId,
            AuthorId = authorId ?? Guid.NewGuid(),
            ViewCount = viewCount,
            HelpfulCount = helpfulCount,
            NotHelpfulCount = notHelpfulCount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PublishedAt = publishedAt ?? (status == ArticleStatus.Published ? DateTime.UtcNow : null)
        };
    }

    public static Tag CreateTestTag(string name = "test-tag")
    {
        return new Tag
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };
    }
}
