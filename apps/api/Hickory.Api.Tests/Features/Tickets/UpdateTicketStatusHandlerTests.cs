using FluentAssertions;
using Hickory.Api.Features.Tickets.UpdateStatus;
using Hickory.Api.Infrastructure.Data.Entities;
using Hickory.Api.Tests.TestUtilities;

namespace Hickory.Api.Tests.Features.Tickets;

public class UpdateTicketStatusHandlerTests
{
    [Theory]
    [InlineData(TicketStatus.Open, TicketStatus.InProgress)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Resolved)]
    [InlineData(TicketStatus.Resolved, TicketStatus.InProgress)]
    public async Task Handle_ValidStatusTransition_UpdatesStatus(TicketStatus currentStatus, TicketStatus newStatus)
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var ticket = TestDataBuilder.CreateTestTicket(status: currentStatus);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateTicketStatusHandler(dbContext);
        var command = new UpdateTicketStatusCommand(ticket.Id, newStatus);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedTicket = await dbContext.Tickets.FindAsync(ticket.Id);
        updatedTicket.Should().NotBeNull();
        updatedTicket!.Status.Should().Be(newStatus);
        updatedTicket.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_NonExistentTicket_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = new UpdateTicketStatusHandler(dbContext);
        var command = new UpdateTicketStatusCommand(Guid.NewGuid(), TicketStatus.InProgress);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain("Ticket with ID");
        exception.Message.Should().Contain("not found");
    }

    [Theory]
    [InlineData(TicketStatus.Closed)]
    [InlineData(TicketStatus.Cancelled)]
    public async Task Handle_ClosedOrCancelledTicket_ThrowsInvalidOperationException(TicketStatus status)
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var ticket = TestDataBuilder.CreateTestTicket(status: status);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateTicketStatusHandler(dbContext);
        var command = new UpdateTicketStatusCommand(ticket.Id, TicketStatus.InProgress);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain($"Cannot change status of {status} ticket");
    }

    [Fact]
    public async Task Handle_TransitionToClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var ticket = TestDataBuilder.CreateTestTicket(status: TicketStatus.Resolved);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateTicketStatusHandler(dbContext);
        var command = new UpdateTicketStatusCommand(ticket.Id, TicketStatus.Closed);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain("Use CloseTicket command");
        exception.Message.Should().Contain("resolution notes");
    }

    [Fact]
    public async Task Handle_OpenToInProgress_UpdatesSuccessfully()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var ticket = TestDataBuilder.CreateTestTicket(status: TicketStatus.Open);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateTicketStatusHandler(dbContext);
        var command = new UpdateTicketStatusCommand(ticket.Id, TicketStatus.InProgress);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedTicket = await dbContext.Tickets.FindAsync(ticket.Id);
        updatedTicket!.Status.Should().Be(TicketStatus.InProgress);
    }

    [Fact]
    public async Task Handle_InProgressToResolved_UpdatesSuccessfully()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var ticket = TestDataBuilder.CreateTestTicket(status: TicketStatus.InProgress);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateTicketStatusHandler(dbContext);
        var command = new UpdateTicketStatusCommand(ticket.Id, TicketStatus.Resolved);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedTicket = await dbContext.Tickets.FindAsync(ticket.Id);
        updatedTicket!.Status.Should().Be(TicketStatus.Resolved);
    }

    [Fact]
    public async Task Handle_ReopenResolvedTicket_UpdatesSuccessfully()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var ticket = TestDataBuilder.CreateTestTicket(status: TicketStatus.Resolved);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateTicketStatusHandler(dbContext);
        var command = new UpdateTicketStatusCommand(ticket.Id, TicketStatus.InProgress);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedTicket = await dbContext.Tickets.FindAsync(ticket.Id);
        updatedTicket!.Status.Should().Be(TicketStatus.InProgress);
    }
}
