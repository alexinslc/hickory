using FluentValidation;

namespace Hickory.Api.Features.Tags.Create;

public class CreateTagValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name is required")
            .MinimumLength(2).WithMessage("Tag name must be at least 2 characters")
            .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9\-]+$").WithMessage("Tag name can only contain letters, numbers, and hyphens");

        RuleFor(x => x.Color)
            .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("Color must be a valid hex color code (e.g., #EF4444)")
            .When(x => !string.IsNullOrEmpty(x.Color));
    }
}
