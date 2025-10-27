using FluentValidation;

namespace Hickory.Api.Features.Comments.Create;

public class AddCommentValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required")
            .MaximumLength(5000).WithMessage("Comment cannot exceed 5000 characters");
    }
}
