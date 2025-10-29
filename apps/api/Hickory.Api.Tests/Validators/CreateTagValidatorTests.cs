using FluentAssertions;
using Hickory.Api.Features.Tags.Create;

namespace Hickory.Api.Tests.Validators;

public class CreateTagValidatorTests
{
    private readonly CreateTagValidator _validator;

    public CreateTagValidatorTests()
    {
        _validator = new CreateTagValidator();
    }

    [Fact]
    public void Validate_ValidRequestWithColor_PassesValidation()
    {
        // Arrange
        var command = new CreateTagCommand(
            Name: "bug-fix",
            Color: "#EF4444"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidRequestWithoutColor_PassesValidation()
    {
        // Arrange
        var command = new CreateTagCommand(
            Name: "feature",
            Color: null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_FailsValidation(string name)
    {
        // Arrange
        var command = new CreateTagCommand(
            Name: name,
            Color: "#EF4444"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage == "Tag name is required");
    }

    [Theory]
    [InlineData("a")]
    [InlineData("x")]
    public void Validate_NameTooShort_FailsValidation(string name)
    {
        // Arrange
        var command = new CreateTagCommand(
            Name: name,
            Color: "#EF4444"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage == "Tag name must be at least 2 characters");
    }

    [Fact]
    public void Validate_NameTooLong_FailsValidation()
    {
        // Arrange
        var longName = new string('a', 51);
        var command = new CreateTagCommand(
            Name: longName,
            Color: "#EF4444"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage == "Tag name cannot exceed 50 characters");
    }

    [Theory]
    [InlineData("bug-fix")]
    [InlineData("feature123")]
    [InlineData("TEST-TAG")]
    [InlineData("tag-with-numbers-123")]
    public void Validate_ValidNameFormat_PassesValidation(string name)
    {
        // Arrange
        var command = new CreateTagCommand(
            Name: name,
            Color: null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("bug fix")]
    [InlineData("tag@name")]
    [InlineData("tag_name")]
    [InlineData("tag.name")]
    public void Validate_InvalidNameFormat_FailsValidation(string name)
    {
        // Arrange
        var command = new CreateTagCommand(
            Name: name,
            Color: null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage == "Tag name can only contain letters, numbers, and hyphens");
    }

    [Theory]
    [InlineData("#FF0000")]
    [InlineData("#00FF00")]
    [InlineData("#0000FF")]
    [InlineData("#EF4444")]
    [InlineData("#3B82F6")]
    [InlineData("#abcdef")]
    [InlineData("#ABCDEF")]
    public void Validate_ValidColorFormat_PassesValidation(string color)
    {
        // Arrange
        var command = new CreateTagCommand(
            Name: "valid-tag",
            Color: color
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("FF0000")]
    [InlineData("#FF00")]
    [InlineData("#FFFF")]
    [InlineData("#FFFFFFFF")]
    [InlineData("red")]
    [InlineData("#GG0000")]
    public void Validate_InvalidColorFormat_FailsValidation(string color)
    {
        // Arrange
        var command = new CreateTagCommand(
            Name: "valid-tag",
            Color: color
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Color" && e.ErrorMessage == "Color must be a valid hex color code (e.g., #EF4444)");
    }

    [Fact]
    public void Validate_EmptyColorString_PassesValidation()
    {
        // Arrange
        var command = new CreateTagCommand(
            Name: "valid-tag",
            Color: ""
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
        var command = new CreateTagCommand(
            Name: "a",
            Color: "invalid"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterOrEqualTo(2);
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
        result.Errors.Should().Contain(e => e.PropertyName == "Color");
    }
}
