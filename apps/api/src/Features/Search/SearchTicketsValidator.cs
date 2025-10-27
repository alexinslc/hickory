using FluentValidation;

namespace Hickory.Api.Features.Search;

public class SearchTicketsValidator : AbstractValidator<SearchTicketsQuery>
{
    public SearchTicketsValidator()
    {
        // Search query must be at least 2 characters if provided
        RuleFor(x => x.SearchQuery)
            .MinimumLength(2)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchQuery))
            .WithMessage("Search query must be at least 2 characters long");

        // Page must be positive
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        // Page size must be between 1 and 100
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");

        // Date range validation
        RuleFor(x => x.CreatedBefore)
            .GreaterThan(x => x.CreatedAfter)
            .When(x => x.CreatedAfter.HasValue && x.CreatedBefore.HasValue)
            .WithMessage("CreatedBefore must be after CreatedAfter");
    }
}
