using FluentAssertions;
using Hickory.Api.Features.Categories.Create;

namespace Hickory.Api.Tests.Validators;

public class CreateCategoryValidatorTests
{
    private readonly CreateCategoryValidator _validator;

    public CreateCategoryValidatorTests()
    {
        _validator = new CreateCategoryValidator();
    }

    [Fact]
    public void Validate_ValidCommandWithAllFields_PassesValidation()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Technical Support",
            Description: "Technical support and troubleshooting",
            DisplayOrder: 1,
            Color: "#3B82F6"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidCommandWithoutOptionalFields_PassesValidation()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Billing",
            Description: null,
            DisplayOrder: 0,
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
        var command = new CreateCategoryCommand(
            Name: name,
            Description: "Valid description",
            DisplayOrder: 1,
            Color: null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage == "Category name is required");
    }

    [Theory]
    [InlineData("A")]
    [InlineData("X")]
    public void Validate_NameTooShort_FailsValidation(string name)
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: name,
            Description: "Valid description",
            DisplayOrder: 1,
            Color: null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage == "Category name must be at least 2 characters");
    }

    [Fact]
    public void Validate_NameTooLong_FailsValidation()
    {
        // Arrange
        var longName = new string('a', 101);
        var command = new CreateCategoryCommand(
            Name: longName,
            Description: "Valid description",
            DisplayOrder: 1,
            Color: null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage == "Category name cannot exceed 100 characters");
    }

    [Fact]
    public void Validate_NameAtMinimumLength_PassesValidation()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "AB",
            Description: null,
            DisplayOrder: 0,
            Color: null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_NameAtMaximumLength_PassesValidation()
    {
        // Arrange
        var maxName = new string('a', 100);
        var command = new CreateCategoryCommand(
            Name: maxName,
            Description: null,
            DisplayOrder: 0,
            Color: null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_DescriptionTooLong_FailsValidation()
    {
        // Arrange
        var longDescription = new string('a', 501);
        var command = new CreateCategoryCommand(
            Name: "Valid Name",
            Description: longDescription,
            DisplayOrder: 1,
            Color: null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description" && e.ErrorMessage == "Description cannot exceed 500 characters");
    }

    [Fact]
    public void Validate_DescriptionAtMaximumLength_PassesValidation()
    {
        // Arrange
        var maxDescription = new string('a', 500);
        var command = new CreateCategoryCommand(
            Name: "Valid Name",
            Description: maxDescription,
            DisplayOrder: 1,
            Color: null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyDescription_PassesValidation()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Valid Name",
            Description: "",
            DisplayOrder: 1,
            Color: null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_NegativeDisplayOrder_FailsValidation(int displayOrder)
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Valid Name",
            Description: null,
            DisplayOrder: displayOrder,
            Color: null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayOrder" && e.ErrorMessage == "Display order must be 0 or greater");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void Validate_ValidDisplayOrder_PassesValidation(int displayOrder)
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Valid Name",
            Description: null,
            DisplayOrder: displayOrder,
            Color: null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("#FF0000")]
    [InlineData("#00FF00")]
    [InlineData("#0000FF")]
    [InlineData("#3B82F6")]
    [InlineData("#EF4444")]
    [InlineData("#abcdef")]
    [InlineData("#ABCDEF")]
    public void Validate_ValidColorFormat_PassesValidation(string color)
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Valid Name",
            Description: null,
            DisplayOrder: 1,
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
    [InlineData("blue")]
    [InlineData("#GG0000")]
    public void Validate_InvalidColorFormat_FailsValidation(string color)
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Valid Name",
            Description: null,
            DisplayOrder: 1,
            Color: color
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Color" && e.ErrorMessage == "Color must be a valid hex color code (e.g. #3B82F6)");
    }

    [Fact]
    public void Validate_EmptyColorString_PassesValidation()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Valid Name",
            Description: null,
            DisplayOrder: 1,
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
        var command = new CreateCategoryCommand(
            Name: "",
            Description: new string('a', 501),
            DisplayOrder: -1,
            Color: "invalid"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterOrEqualTo(4);
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayOrder");
        result.Errors.Should().Contain(e => e.PropertyName == "Color");
    }
}
