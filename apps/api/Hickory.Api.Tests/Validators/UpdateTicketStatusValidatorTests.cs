using FluentAssertions;
using Hickory.Api.Features.Tickets.UpdateStatus;
using Hickory.Api.Infrastructure.Data.Entities;

namespace Hickory.Api.Tests.Validators;

public class UpdateTicketStatusValidatorTests
{
    private readonly UpdateTicketStatusValidator _validator;

    public UpdateTicketStatusValidatorTests()
    {
        _validator = new UpdateTicketStatusValidator();
    }

    [Theory]
    [InlineData(TicketStatus.Open)]
    [InlineData(TicketStatus.InProgress)]
    [InlineData(TicketStatus.Resolved)]
    [InlineData(TicketStatus.Cancelled)]
    public void Validate_ValidStatusExceptClosed_PassesValidation(TicketStatus status)
    {
        // Arrange
        var command = new UpdateTicketStatusCommand(
            TicketId: Guid.NewGuid(),
            NewStatus: status
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyTicketId_FailsValidation()
    {
        // Arrange
        var command = new UpdateTicketStatusCommand(
            TicketId: Guid.Empty,
            NewStatus: TicketStatus.InProgress
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TicketId" && e.ErrorMessage == "Ticket ID is required");
    }

    [Fact]
    public void Validate_ClosedStatus_FailsValidation()
    {
        // Arrange
        var command = new UpdateTicketStatusCommand(
            TicketId: Guid.NewGuid(),
            NewStatus: TicketStatus.Closed
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewStatus" && e.ErrorMessage == "Use CloseTicket command to close a ticket with resolution notes");
    }

    [Fact]
    public void Validate_OpenStatus_PassesValidation()
    {
        // Arrange
        var command = new UpdateTicketStatusCommand(
            TicketId: Guid.NewGuid(),
            NewStatus: TicketStatus.Open
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_InProgressStatus_PassesValidation()
    {
        // Arrange
        var command = new UpdateTicketStatusCommand(
            TicketId: Guid.NewGuid(),
            NewStatus: TicketStatus.InProgress
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ResolvedStatus_PassesValidation()
    {
        // Arrange
        var command = new UpdateTicketStatusCommand(
            TicketId: Guid.NewGuid(),
            NewStatus: TicketStatus.Resolved
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_CancelledStatus_PassesValidation()
    {
        // Arrange
        var command = new UpdateTicketStatusCommand(
            TicketId: Guid.NewGuid(),
            NewStatus: TicketStatus.Cancelled
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var command = new UpdateTicketStatusCommand(
            TicketId: Guid.Empty,
            NewStatus: TicketStatus.Closed
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterOrEqualTo(2);
        result.Errors.Should().Contain(e => e.PropertyName == "TicketId");
        result.Errors.Should().Contain(e => e.PropertyName == "NewStatus");
    }
}
