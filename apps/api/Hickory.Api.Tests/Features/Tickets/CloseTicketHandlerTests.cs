using FluentAssertions;
using Hickory.Api.Features.Tickets.Close;
using Hickory.Api.Infrastructure.Data.Entities;
using Hickory.Api.Tests.TestUtilities;

namespace Hickory.Api.Tests.Features.Tickets;

public class CloseTicketHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_ClosesTicket()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var ticket = TestDataBuilder.CreateTestTicket(status: TicketStatus.InProgress);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new CloseTicketHandler(dbContext);
        var command = new CloseTicketCommand(ticket.Id, "Issue resolved successfully");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var closedTicket = await dbContext.Tickets.FindAsync(ticket.Id);
        closedTicket.Should().NotBeNull();
        closedTicket!.Status.Should().Be(TicketStatus.Closed);
        closedTicket.ResolutionNotes.Should().Be("Issue resolved successfully");
        closedTicket.ClosedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        closedTicket.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_OpenTicket_CanBeClosed()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var ticket = TestDataBuilder.CreateTestTicket(status: TicketStatus.Open);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new CloseTicketHandler(dbContext);
        var command = new CloseTicketCommand(ticket.Id, "Resolved without assignment");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var closedTicket = await dbContext.Tickets.FindAsync(ticket.Id);
        closedTicket!.Status.Should().Be(TicketStatus.Closed);
    }

    [Fact]
    public async Task Handle_NonExistentTicket_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = new CloseTicketHandler(dbContext);
        var command = new CloseTicketCommand(Guid.NewGuid(), "Resolution notes");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain("Ticket with ID");
        exception.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_AlreadyClosedTicket_ThrowsInvalidOperationException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var ticket = TestDataBuilder.CreateTestTicket(status: TicketStatus.Closed);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new CloseTicketHandler(dbContext);
        var command = new CloseTicketCommand(ticket.Id, "Resolution notes");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain("already Closed");
    }

    [Fact]
    public async Task Handle_CancelledTicket_ThrowsInvalidOperationException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var ticket = TestDataBuilder.CreateTestTicket(status: TicketStatus.Cancelled);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new CloseTicketHandler(dbContext);
        var command = new CloseTicketCommand(ticket.Id, "Resolution notes");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain("already Cancelled");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Handle_EmptyResolutionNotes_ThrowsArgumentException(string resolutionNotes)
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var ticket = TestDataBuilder.CreateTestTicket(status: TicketStatus.InProgress);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new CloseTicketHandler(dbContext);
        var command = new CloseTicketCommand(ticket.Id, resolutionNotes);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain("Resolution notes are required");
    }

    [Fact]
    public async Task Handle_ValidRequest_SetsResolutionNotes()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var ticket = TestDataBuilder.CreateTestTicket(status: TicketStatus.Resolved);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new CloseTicketHandler(dbContext);
        var resolutionNotes = "Customer issue resolved by restarting the service. No further action needed.";
        var command = new CloseTicketCommand(ticket.Id, resolutionNotes);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var closedTicket = await dbContext.Tickets.FindAsync(ticket.Id);
        closedTicket!.ResolutionNotes.Should().Be(resolutionNotes);
    }
}
