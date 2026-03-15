using FluentAssertions;
using Hickory.Api.Features.Users.DataExport;
using Hickory.Api.Infrastructure.Data.Entities;
using Hickory.Api.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Tests.Features.Users;

public class ExportUserDataHandlerTests
{
    [Fact]
    public async Task Handle_ValidUser_ReturnsAllUserData()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser(
            email: "john@example.com",
            firstName: "John",
            lastName: "Doe"
        );
        dbContext.Users.Add(user);

        var ticket = TestDataBuilder.CreateTestTicket(
            submitterId: user.Id,
            title: "My Issue",
            description: "Something is broken"
        );
        dbContext.Tickets.Add(ticket);

        var comment = TestDataBuilder.CreateTestComment(
            ticketId: ticket.Id,
            authorId: user.Id,
            content: "Here are more details"
        );
        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync();

        var handler = new ExportUserDataHandler(dbContext);
        var query = new ExportUserDataQuery(user.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Profile.Email.Should().Be("john@example.com");
        result.Profile.FirstName.Should().Be("John");
        result.Profile.LastName.Should().Be("Doe");
        result.Tickets.Should().HaveCount(1);
        result.Tickets[0].Title.Should().Be("My Issue");
        result.Comments.Should().HaveCount(1);
        result.Comments[0].Content.Should().Be("Here are more details");
        result.ExportedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_UserWithNoTicketsOrComments_ReturnsEmptyCollections()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new ExportUserDataHandler(dbContext);
        var query = new ExportUserDataQuery(user.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Profile.Should().NotBeNull();
        result.Tickets.Should().BeEmpty();
        result.Comments.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NonExistentUser_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = new ExportUserDataHandler(dbContext);
        var query = new ExportUserDataQuery(Guid.NewGuid());

        // Act
        var act = () => handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_OnlyReturnsOwnTickets_NotOtherUsers()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser(email: "user1@example.com");
        var otherUser = TestDataBuilder.CreateTestUser(email: "user2@example.com");
        dbContext.Users.AddRange(user, otherUser);

        var myTicket = TestDataBuilder.CreateTestTicket(submitterId: user.Id, title: "My Ticket");
        var otherTicket = TestDataBuilder.CreateTestTicket(submitterId: otherUser.Id, title: "Other Ticket", ticketNumber: "TKT-00002");
        dbContext.Tickets.AddRange(myTicket, otherTicket);
        await dbContext.SaveChangesAsync();

        var handler = new ExportUserDataHandler(dbContext);
        var query = new ExportUserDataQuery(user.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tickets.Should().HaveCount(1);
        result.Tickets[0].Title.Should().Be("My Ticket");
    }
}
