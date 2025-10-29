using FluentAssertions;
using Hickory.Api.Common.Services;
using Hickory.Api.Features.Tickets.Create;
using Hickory.Api.Features.Tickets.Create.Models;
using Hickory.Api.Infrastructure.Data.Entities;
using Hickory.Api.Tests.TestUtilities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Hickory.Api.Tests.Features.Tickets;

public class CreateTicketHandlerTests
{
    private readonly Mock<ITicketNumberGenerator> _ticketNumberGeneratorMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;

    public CreateTicketHandlerTests()
    {
        _ticketNumberGeneratorMock = new Mock<ITicketNumberGenerator>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        // Setup default mock behavior
        _ticketNumberGeneratorMock
            .Setup(g => g.GenerateTicketNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("TKT-00001");
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesTicket()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new CreateTicketHandler(
            dbContext,
            _ticketNumberGeneratorMock.Object,
            _publishEndpointMock.Object
        );

        var request = new CreateTicketRequest
        {
            Title = "Test Issue",
            Description = "This is a test description",
            Priority = "High",
            CategoryId = null
        };

        var command = new CreateTicketCommand(request, user.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TicketNumber.Should().Be("TKT-00001");
        result.Title.Should().Be("Test Issue");
        result.Description.Should().Be("This is a test description");
        result.Status.Should().Be("Open");
        result.Priority.Should().Be("High");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify ticket was saved to database
        var savedTicket = await dbContext.Tickets.FirstOrDefaultAsync(t => t.TicketNumber == "TKT-00001");
        savedTicket.Should().NotBeNull();
        savedTicket!.SubmitterId.Should().Be(user.Id);
        savedTicket.Status.Should().Be(TicketStatus.Open);
    }

    [Theory]
    [InlineData("Low", TicketPriority.Low)]
    [InlineData("Medium", TicketPriority.Medium)]
    [InlineData("High", TicketPriority.High)]
    [InlineData("Critical", TicketPriority.Critical)]
    public async Task Handle_DifferentPriorities_CreatesTicketWithCorrectPriority(string priorityString, TicketPriority expectedPriority)
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new CreateTicketHandler(
            dbContext,
            _ticketNumberGeneratorMock.Object,
            _publishEndpointMock.Object
        );

        var request = new CreateTicketRequest
        {
            Title = "Test",
            Description = "Test",
            Priority = priorityString
        };

        var command = new CreateTicketCommand(request, user.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var savedTicket = await dbContext.Tickets.FirstOrDefaultAsync(t => t.Id == result.Id);
        savedTicket!.Priority.Should().Be(expectedPriority);
    }

    [Fact]
    public async Task Handle_InvalidPriority_DefaultsToMedium()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new CreateTicketHandler(
            dbContext,
            _ticketNumberGeneratorMock.Object,
            _publishEndpointMock.Object
        );

        var request = new CreateTicketRequest
        {
            Title = "Test",
            Description = "Test",
            Priority = "InvalidPriority"
        };

        var command = new CreateTicketCommand(request, user.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var savedTicket = await dbContext.Tickets.FirstOrDefaultAsync(t => t.Id == result.Id);
        savedTicket!.Priority.Should().Be(TicketPriority.Medium);
    }

    [Fact]
    public async Task Handle_WithCategoryId_AssignsCategory()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser();
        var category = TestDataBuilder.CreateTestCategory(name: "Technical Support");
        dbContext.Users.Add(user);
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();

        var handler = new CreateTicketHandler(
            dbContext,
            _ticketNumberGeneratorMock.Object,
            _publishEndpointMock.Object
        );

        var request = new CreateTicketRequest
        {
            Title = "Test",
            Description = "Test",
            Priority = "Medium",
            CategoryId = category.Id
        };

        var command = new CreateTicketCommand(request, user.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var savedTicket = await dbContext.Tickets.FirstOrDefaultAsync(t => t.Id == result.Id);
        savedTicket!.CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public async Task Handle_ValidRequest_GeneratesTicketNumber()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new CreateTicketHandler(
            dbContext,
            _ticketNumberGeneratorMock.Object,
            _publishEndpointMock.Object
        );

        var request = new CreateTicketRequest
        {
            Title = "Test",
            Description = "Test",
            Priority = "Medium"
        };

        var command = new CreateTicketCommand(request, user.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _ticketNumberGeneratorMock.Verify(
            g => g.GenerateTicketNumberAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ValidRequest_PublishesTicketCreatedEvent()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser(
            email: "test@example.com",
            firstName: "John",
            lastName: "Doe"
        );
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new CreateTicketHandler(
            dbContext,
            _ticketNumberGeneratorMock.Object,
            _publishEndpointMock.Object
        );

        var request = new CreateTicketRequest
        {
            Title = "Test Issue",
            Description = "Test Description",
            Priority = "High"
        };

        var command = new CreateTicketCommand(request, user.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _publishEndpointMock.Verify(
            p => p.Publish(
                It.Is<Common.Events.TicketCreatedEvent>(e =>
                    e.TicketNumber == "TKT-00001" &&
                    e.Title == "Test Issue" &&
                    e.Description == "Test Description" &&
                    e.Status == "Open" &&
                    e.Priority == "High" &&
                    e.SubmitterId == user.Id &&
                    e.SubmitterName == "John Doe" &&
                    e.SubmitterEmail == "test@example.com"
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ValidRequest_SetsCorrectDefaults()
    {
        // Arrange
        var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var user = TestDataBuilder.CreateTestUser();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new CreateTicketHandler(
            dbContext,
            _ticketNumberGeneratorMock.Object,
            _publishEndpointMock.Object
        );

        var request = new CreateTicketRequest
        {
            Title = "Test",
            Description = "Test",
            Priority = "Medium"
        };

        var command = new CreateTicketCommand(request, user.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var savedTicket = await dbContext.Tickets.FirstOrDefaultAsync(t => t.Id == result.Id);
        savedTicket!.Status.Should().Be(TicketStatus.Open);
        savedTicket.AssignedToId.Should().BeNull();
        savedTicket.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        savedTicket.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
