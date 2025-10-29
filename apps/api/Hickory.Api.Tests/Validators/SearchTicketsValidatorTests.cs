using FluentAssertions;
using Hickory.Api.Features.Search;
using Hickory.Api.Infrastructure.Data.Entities;

namespace Hickory.Api.Tests.Validators;

public class SearchTicketsValidatorTests
{
    private readonly SearchTicketsValidator _validator;

    public SearchTicketsValidatorTests()
    {
        _validator = new SearchTicketsValidator();
    }

    [Fact]
    public void Validate_ValidRequestWithAllParameters_PassesValidation()
    {
        // Arrange
        var query = new SearchTicketsQuery(
            SearchQuery: "test query",
            UserId: Guid.NewGuid(),
            UserRole: "Agent",
            Status: TicketStatus.Open,
            Priority: TicketPriority.High,
            AssignedToId: Guid.NewGuid(),
            CreatedAfter: DateTime.UtcNow.AddDays(-30),
            CreatedBefore: DateTime.UtcNow,
            Page: 1,
            PageSize: 20
        );

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidRequestWithMinimalParameters_PassesValidation()
    {
        // Arrange
        var query = new SearchTicketsQuery(
            SearchQuery: null,
            UserId: Guid.NewGuid(),
            UserRole: "User",
            Status: null,
            Priority: null,
            AssignedToId: null,
            CreatedAfter: null,
            CreatedBefore: null,
            Page: 1,
            PageSize: 20
        );

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("a")]
    [InlineData("x")]
    public void Validate_SearchQueryTooShort_FailsValidation(string searchQuery)
    {
        // Arrange
        var query = new SearchTicketsQuery(
            SearchQuery: searchQuery,
            UserId: Guid.NewGuid(),
            UserRole: "Agent",
            Status: null,
            Priority: null,
            AssignedToId: null,
            CreatedAfter: null,
            CreatedBefore: null,
            Page: 1,
            PageSize: 20
        );

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SearchQuery" && e.ErrorMessage == "Search query must be at least 2 characters long");
    }

    [Fact]
    public void Validate_EmptySearchQuery_PassesValidation()
    {
        // Arrange
        var query = new SearchTicketsQuery(
            SearchQuery: "",
            UserId: Guid.NewGuid(),
            UserRole: "Agent",
            Status: null,
            Priority: null,
            AssignedToId: null,
            CreatedAfter: null,
            CreatedBefore: null,
            Page: 1,
            PageSize: 20
        );

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_InvalidPage_FailsValidation(int page)
    {
        // Arrange
        var query = new SearchTicketsQuery(
            SearchQuery: null,
            UserId: Guid.NewGuid(),
            UserRole: "Agent",
            Status: null,
            Priority: null,
            AssignedToId: null,
            CreatedAfter: null,
            CreatedBefore: null,
            Page: page,
            PageSize: 20
        );

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Page" && e.ErrorMessage == "Page must be greater than 0");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void Validate_ValidPage_PassesValidation(int page)
    {
        // Arrange
        var query = new SearchTicketsQuery(
            SearchQuery: null,
            UserId: Guid.NewGuid(),
            UserRole: "Agent",
            Status: null,
            Priority: null,
            AssignedToId: null,
            CreatedAfter: null,
            CreatedBefore: null,
            Page: page,
            PageSize: 20
        );

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(150)]
    public void Validate_InvalidPageSize_FailsValidation(int pageSize)
    {
        // Arrange
        var query = new SearchTicketsQuery(
            SearchQuery: null,
            UserId: Guid.NewGuid(),
            UserRole: "Agent",
            Status: null,
            Priority: null,
            AssignedToId: null,
            CreatedAfter: null,
            CreatedBefore: null,
            Page: 1,
            PageSize: pageSize
        );

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageSize" && e.ErrorMessage == "Page size must be between 1 and 100");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_ValidPageSize_PassesValidation(int pageSize)
    {
        // Arrange
        var query = new SearchTicketsQuery(
            SearchQuery: null,
            UserId: Guid.NewGuid(),
            UserRole: "Agent",
            Status: null,
            Priority: null,
            AssignedToId: null,
            CreatedAfter: null,
            CreatedBefore: null,
            Page: 1,
            PageSize: pageSize
        );

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_CreatedBeforeEarlierThanCreatedAfter_FailsValidation()
    {
        // Arrange
        var query = new SearchTicketsQuery(
            SearchQuery: null,
            UserId: Guid.NewGuid(),
            UserRole: "Agent",
            Status: null,
            Priority: null,
            AssignedToId: null,
            CreatedAfter: DateTime.UtcNow,
            CreatedBefore: DateTime.UtcNow.AddDays(-30),
            Page: 1,
            PageSize: 20
        );

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CreatedBefore" && e.ErrorMessage == "CreatedBefore must be after CreatedAfter");
    }

    [Fact]
    public void Validate_CreatedBeforeAfterCreatedAfter_PassesValidation()
    {
        // Arrange
        var query = new SearchTicketsQuery(
            SearchQuery: null,
            UserId: Guid.NewGuid(),
            UserRole: "Agent",
            Status: null,
            Priority: null,
            AssignedToId: null,
            CreatedAfter: DateTime.UtcNow.AddDays(-30),
            CreatedBefore: DateTime.UtcNow,
            Page: 1,
            PageSize: 20
        );

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_OnlyCreatedAfter_PassesValidation()
    {
        // Arrange
        var query = new SearchTicketsQuery(
            SearchQuery: null,
            UserId: Guid.NewGuid(),
            UserRole: "Agent",
            Status: null,
            Priority: null,
            AssignedToId: null,
            CreatedAfter: DateTime.UtcNow.AddDays(-30),
            CreatedBefore: null,
            Page: 1,
            PageSize: 20
        );

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_OnlyCreatedBefore_PassesValidation()
    {
        // Arrange
        var query = new SearchTicketsQuery(
            SearchQuery: null,
            UserId: Guid.NewGuid(),
            UserRole: "Agent",
            Status: null,
            Priority: null,
            AssignedToId: null,
            CreatedAfter: null,
            CreatedBefore: DateTime.UtcNow,
            Page: 1,
            PageSize: 20
        );

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var query = new SearchTicketsQuery(
            SearchQuery: "a",
            UserId: Guid.NewGuid(),
            UserRole: "Agent",
            Status: null,
            Priority: null,
            AssignedToId: null,
            CreatedAfter: DateTime.UtcNow,
            CreatedBefore: DateTime.UtcNow.AddDays(-30),
            Page: 0,
            PageSize: 150
        );

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterOrEqualTo(4);
        result.Errors.Should().Contain(e => e.PropertyName == "SearchQuery");
        result.Errors.Should().Contain(e => e.PropertyName == "Page");
        result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
        result.Errors.Should().Contain(e => e.PropertyName == "CreatedBefore");
    }
}
