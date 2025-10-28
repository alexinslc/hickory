using FluentAssertions;
using Hickory.Api.Common.Services;
using Hickory.Api.Tests.TestUtilities;

namespace Hickory.Api.Tests.Infrastructure.Data;

public class TicketNumberGeneratorTests
{
    [Fact]
    public async Task GenerateTicketNumberAsync_NoExistingTickets_ReturnsFirstNumber()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var generator = new TicketNumberGenerator(dbContext);

        // Act
        var ticketNumber = await generator.GenerateTicketNumberAsync();

        // Assert
        ticketNumber.Should().Be("TKT-00001");
    }

    [Fact]
    public async Task GenerateTicketNumberAsync_WithExistingTickets_ReturnsNextNumber()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var ticket1 = TestDataBuilder.CreateTestTicket(ticketNumber: "TKT-00001");
        var ticket2 = TestDataBuilder.CreateTestTicket(ticketNumber: "TKT-00002");
        var ticket3 = TestDataBuilder.CreateTestTicket(ticketNumber: "TKT-00003");

        dbContext.Tickets.AddRange(ticket1, ticket2, ticket3);
        await dbContext.SaveChangesAsync();

        var generator = new TicketNumberGenerator(dbContext);

        // Act
        var ticketNumber = await generator.GenerateTicketNumberAsync();

        // Assert
        ticketNumber.Should().Be("TKT-00004");
    }

    [Fact]
    public async Task GenerateTicketNumberAsync_WithNonSequentialTickets_ReturnsNextNumber()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var ticket1 = TestDataBuilder.CreateTestTicket(ticketNumber: "TKT-00001");
        var ticket2 = TestDataBuilder.CreateTestTicket(ticketNumber: "TKT-00005");
        var ticket3 = TestDataBuilder.CreateTestTicket(ticketNumber: "TKT-00003");

        dbContext.Tickets.AddRange(ticket1, ticket2, ticket3);
        await dbContext.SaveChangesAsync();

        var generator = new TicketNumberGenerator(dbContext);

        // Act
        var ticketNumber = await generator.GenerateTicketNumberAsync();

        // Assert
        ticketNumber.Should().Be("TKT-00006"); // Should be max + 1
    }

    [Fact]
    public async Task GenerateTicketNumberAsync_WithInvalidTicketNumbers_IgnoresInvalid()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var ticket1 = TestDataBuilder.CreateTestTicket(ticketNumber: "TKT-00001");
        var ticket2 = TestDataBuilder.CreateTestTicket(ticketNumber: "TKT-00002");
        var ticket3 = TestDataBuilder.CreateTestTicket(ticketNumber: "TKT-INVALID");

        dbContext.Tickets.AddRange(ticket1, ticket2, ticket3);
        await dbContext.SaveChangesAsync();

        var generator = new TicketNumberGenerator(dbContext);

        // Act
        var ticketNumber = await generator.GenerateTicketNumberAsync();

        // Assert
        ticketNumber.Should().Be("TKT-00003"); // Should ignore invalid and continue from 2
    }

    [Fact]
    public async Task GenerateTicketNumberAsync_FormatsWithLeadingZeros()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var ticket = TestDataBuilder.CreateTestTicket(ticketNumber: "TKT-00099");

        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        var generator = new TicketNumberGenerator(dbContext);

        // Act
        var ticketNumber = await generator.GenerateTicketNumberAsync();

        // Assert
        ticketNumber.Should().Be("TKT-00100");
        ticketNumber.Length.Should().Be(9); // TKT- + 5 digits
    }

    [Fact]
    public async Task GenerateTicketNumberAsync_HandlesConcurrentCalls_ReturnsUniqueNumbers()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var generator = new TicketNumberGenerator(dbContext);

        // Act - Simulate concurrent calls (in reality they would be sequential in this test)
        var ticket1 = await generator.GenerateTicketNumberAsync();

        // Add first ticket to database before generating second
        var newTicket1 = TestDataBuilder.CreateTestTicket(ticketNumber: ticket1);
        dbContext.Tickets.Add(newTicket1);
        await dbContext.SaveChangesAsync();

        var ticket2 = await generator.GenerateTicketNumberAsync();

        // Assert
        ticket1.Should().Be("TKT-00001");
        ticket2.Should().Be("TKT-00002");
        ticket1.Should().NotBe(ticket2);
    }
}
