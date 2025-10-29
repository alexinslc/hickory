using FluentAssertions;
using Hickory.Api.Features.Tickets.Close;

namespace Hickory.Api.Tests.Validators;

public class CloseTicketValidatorTests
{
    private readonly CloseTicketValidator _validator;

    public CloseTicketValidatorTests()
    {
        _validator = new CloseTicketValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new CloseTicketCommand(
            TicketId: Guid.NewGuid(),
            ResolutionNotes: "The issue has been resolved by updating the configuration."
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
        var command = new CloseTicketCommand(
            TicketId: Guid.Empty,
            ResolutionNotes: "The issue has been resolved."
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TicketId" && e.ErrorMessage == "Ticket ID is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyResolutionNotes_FailsValidation(string resolutionNotes)
    {
        // Arrange
        var command = new CloseTicketCommand(
            TicketId: Guid.NewGuid(),
            ResolutionNotes: resolutionNotes
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ResolutionNotes" && e.ErrorMessage == "Resolution notes are required when closing a ticket");
    }

    [Theory]
    [InlineData("Too short")]
    [InlineData("12345678")]
    public void Validate_ResolutionNotesTooShort_FailsValidation(string resolutionNotes)
    {
        // Arrange
        var command = new CloseTicketCommand(
            TicketId: Guid.NewGuid(),
            ResolutionNotes: resolutionNotes
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ResolutionNotes" && e.ErrorMessage == "Resolution notes must be at least 10 characters");
    }

    [Fact]
    public void Validate_ResolutionNotesTooLong_FailsValidation()
    {
        // Arrange
        var longNotes = new string('a', 5001);
        var command = new CloseTicketCommand(
            TicketId: Guid.NewGuid(),
            ResolutionNotes: longNotes
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ResolutionNotes" && e.ErrorMessage == "Resolution notes cannot exceed 5000 characters");
    }

    [Fact]
    public void Validate_ResolutionNotesAtMinimumLength_PassesValidation()
    {
        // Arrange
        var command = new CloseTicketCommand(
            TicketId: Guid.NewGuid(),
            ResolutionNotes: "1234567890"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ResolutionNotesAtMaximumLength_PassesValidation()
    {
        // Arrange
        var maxNotes = new string('a', 5000);
        var command = new CloseTicketCommand(
            TicketId: Guid.NewGuid(),
            ResolutionNotes: maxNotes
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
        var command = new CloseTicketCommand(
            TicketId: Guid.Empty,
            ResolutionNotes: "short"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterOrEqualTo(2);
        result.Errors.Should().Contain(e => e.PropertyName == "TicketId");
        result.Errors.Should().Contain(e => e.PropertyName == "ResolutionNotes");
    }
}
