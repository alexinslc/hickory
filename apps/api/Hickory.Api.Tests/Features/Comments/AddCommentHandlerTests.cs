using FluentAssertions;
using Hickory.Api.Features.Comments.Create;
using Hickory.Api.Infrastructure.Data.Entities;
using Hickory.Api.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Tests.Features.Comments;

public class AddCommentHandlerTests
{
    [Fact]
    public async Task Handle_ValidComment_AddsCommentToTicket()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser();
        var ticket = TestDataBuilder.CreateTestTicket();
        dbContext.Users.Add(user);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new AddCommentHandler(dbContext);
        var request = new AddCommentRequest
        {
            Content = "This is a test comment",
            IsInternal = false
        };
        var command = new AddCommentCommand(ticket.Id, request, user.Id, "User");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be("This is a test comment");
        result.IsInternal.Should().BeFalse();
        result.AuthorId.Should().Be(user.Id);
        result.AuthorName.Should().Be($"{user.FirstName} {user.LastName}");

        var savedComment = await dbContext.Comments
            .FirstOrDefaultAsync(c => c.Id == result.Id);
        savedComment.Should().NotBeNull();
        savedComment!.TicketId.Should().Be(ticket.Id);
    }

    [Fact]
    public async Task Handle_AgentInternalNote_CreatesInternalComment()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var agent = TestDataBuilder.CreateTestUser(role: UserRole.Agent);
        var ticket = TestDataBuilder.CreateTestTicket();
        dbContext.Users.Add(agent);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new AddCommentHandler(dbContext);
        var request = new AddCommentRequest
        {
            Content = "Internal agent note",
            IsInternal = true
        };
        var command = new AddCommentCommand(ticket.Id, request, agent.Id, "Agent");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsInternal.Should().BeTrue();

        var savedComment = await dbContext.Comments.FindAsync(result.Id);
        savedComment!.IsInternal.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_RegularUserInternalNote_CreatesPublicComment()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser(role: UserRole.EndUser);
        var ticket = TestDataBuilder.CreateTestTicket();
        dbContext.Users.Add(user);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new AddCommentHandler(dbContext);
        var request = new AddCommentRequest
        {
            Content = "User attempting internal note",
            IsInternal = true  // User requests internal, but should be ignored
        };
        var command = new AddCommentCommand(ticket.Id, request, user.Id, "User");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsInternal.Should().BeFalse();  // Should be forced to public

        var savedComment = await dbContext.Comments.FindAsync(result.Id);
        savedComment!.IsInternal.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_AdminInternalNote_CreatesInternalComment()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var admin = TestDataBuilder.CreateTestUser(role: UserRole.Administrator);
        var ticket = TestDataBuilder.CreateTestTicket();
        dbContext.Users.Add(admin);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new AddCommentHandler(dbContext);
        var request = new AddCommentRequest
        {
            Content = "Admin internal note",
            IsInternal = true
        };
        var command = new AddCommentCommand(ticket.Id, request, admin.Id, "Admin");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsInternal.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NonExistentTicket_ThrowsInvalidOperationException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new AddCommentHandler(dbContext);
        var request = new AddCommentRequest { Content = "Test comment" };
        var command = new AddCommentCommand(Guid.NewGuid(), request, user.Id, "User");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain("Ticket not found");
    }

    [Fact]
    public async Task Handle_ValidComment_SetsCreatedTimestamp()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser();
        var ticket = TestDataBuilder.CreateTestTicket();
        dbContext.Users.Add(user);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new AddCommentHandler(dbContext);
        var request = new AddCommentRequest { Content = "Test comment" };
        var command = new AddCommentCommand(ticket.Id, request, user.Id, "User");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_MultipleComments_AllSavedToTicket()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser();
        var ticket = TestDataBuilder.CreateTestTicket();
        dbContext.Users.Add(user);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new AddCommentHandler(dbContext);

        // Act - Add 3 comments
        var command1 = new AddCommentCommand(ticket.Id, new AddCommentRequest { Content = "Comment 1" }, user.Id, "User");
        var command2 = new AddCommentCommand(ticket.Id, new AddCommentRequest { Content = "Comment 2" }, user.Id, "User");
        var command3 = new AddCommentCommand(ticket.Id, new AddCommentRequest { Content = "Comment 3" }, user.Id, "User");

        await handler.Handle(command1, CancellationToken.None);
        await handler.Handle(command2, CancellationToken.None);
        await handler.Handle(command3, CancellationToken.None);

        // Assert
        var comments = await dbContext.Comments
            .Where(c => c.TicketId == ticket.Id)
            .ToListAsync();

        comments.Should().HaveCount(3);
        comments.Select(c => c.Content).Should().BeEquivalentTo("Comment 1", "Comment 2", "Comment 3");
    }
}
