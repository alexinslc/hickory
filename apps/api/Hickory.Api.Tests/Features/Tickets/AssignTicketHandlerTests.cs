using FluentAssertions;
using Hickory.Api.Features.Tickets.Assign;
using Hickory.Api.Infrastructure.Data.Entities;
using Hickory.Api.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Tests.Features.Tickets;

public class AssignTicketHandlerTests
{
    [Fact]
    public async Task Handle_ValidAssignment_AssignsTicketToAgent()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var agent = TestDataBuilder.CreateTestUser(
            email: "agent@example.com",
            role: UserRole.Agent
        );
        var ticket = TestDataBuilder.CreateTestTicket(
            status: TicketStatus.Open,
            assignedToId: null
        );

        dbContext.Users.Add(agent);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new AssignTicketHandler(dbContext);
        var command = new AssignTicketCommand(ticket.Id, agent.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedTicket = await dbContext.Tickets.FindAsync(ticket.Id);
        updatedTicket.Should().NotBeNull();
        updatedTicket!.AssignedToId.Should().Be(agent.Id);
        updatedTicket.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_OpenTicket_ChangesStatusToInProgress()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var agent = TestDataBuilder.CreateTestUser(role: UserRole.Agent);
        var ticket = TestDataBuilder.CreateTestTicket(status: TicketStatus.Open);

        dbContext.Users.Add(agent);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new AssignTicketHandler(dbContext);
        var command = new AssignTicketCommand(ticket.Id, agent.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedTicket = await dbContext.Tickets.FindAsync(ticket.Id);
        updatedTicket!.Status.Should().Be(TicketStatus.InProgress);
    }

    [Fact]
    public async Task Handle_AlreadyInProgressTicket_DoesNotChangeStatus()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var agent1 = TestDataBuilder.CreateTestUser(email: "agent1@example.com", role: UserRole.Agent);
        var agent2 = TestDataBuilder.CreateTestUser(email: "agent2@example.com", role: UserRole.Agent);
        var ticket = TestDataBuilder.CreateTestTicket(
            status: TicketStatus.InProgress,
            assignedToId: agent1.Id
        );

        dbContext.Users.AddRange(agent1, agent2);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new AssignTicketHandler(dbContext);
        var command = new AssignTicketCommand(ticket.Id, agent2.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedTicket = await dbContext.Tickets.FindAsync(ticket.Id);
        updatedTicket!.Status.Should().Be(TicketStatus.InProgress);
        updatedTicket.AssignedToId.Should().Be(agent2.Id);
    }

    [Fact]
    public async Task Handle_NonExistentTicket_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var agent = TestDataBuilder.CreateTestUser(role: UserRole.Agent);
        dbContext.Users.Add(agent);
        await dbContext.SaveChangesAsync();

        var handler = new AssignTicketHandler(dbContext);
        var command = new AssignTicketCommand(Guid.NewGuid(), agent.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain("Ticket with ID");
        exception.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_NonExistentAgent_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var ticket = TestDataBuilder.CreateTestTicket();
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new AssignTicketHandler(dbContext);
        var command = new AssignTicketCommand(ticket.Id, Guid.NewGuid());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain("Agent with ID");
        exception.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_UserWithoutAgentRole_ThrowsInvalidOperationException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var regularUser = TestDataBuilder.CreateTestUser(
            email: "user@example.com",
            role: UserRole.EndUser
        );
        var ticket = TestDataBuilder.CreateTestTicket();

        dbContext.Users.Add(regularUser);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new AssignTicketHandler(dbContext);
        var command = new AssignTicketCommand(ticket.Id, regularUser.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None)
        );
        exception.Message.Should().Contain("Agent or Administrator role");
    }

    [Fact]
    public async Task Handle_AdministratorRole_CanBeAssignedTickets()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var admin = TestDataBuilder.CreateTestUser(
            email: "admin@example.com",
            role: UserRole.Administrator
        );
        var ticket = TestDataBuilder.CreateTestTicket();

        dbContext.Users.Add(admin);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new AssignTicketHandler(dbContext);
        var command = new AssignTicketCommand(ticket.Id, admin.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedTicket = await dbContext.Tickets.FindAsync(ticket.Id);
        updatedTicket!.AssignedToId.Should().Be(admin.Id);
    }

    [Fact]
    public async Task Handle_ReassignTicket_UpdatesAssignment()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var agent1 = TestDataBuilder.CreateTestUser(email: "agent1@example.com", role: UserRole.Agent);
        var agent2 = TestDataBuilder.CreateTestUser(email: "agent2@example.com", role: UserRole.Agent);
        var ticket = TestDataBuilder.CreateTestTicket(assignedToId: agent1.Id);

        dbContext.Users.AddRange(agent1, agent2);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var handler = new AssignTicketHandler(dbContext);
        var command = new AssignTicketCommand(ticket.Id, agent2.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedTicket = await dbContext.Tickets.FindAsync(ticket.Id);
        updatedTicket!.AssignedToId.Should().Be(agent2.Id);
    }
}
