using FluentValidation;
using Hickory.Api.Features.Tickets.Create.Models;

namespace Hickory.Api.Features.Tickets.Create;

public class CreateTicketValidator : AbstractValidator<CreateTicketRequest>
{
    public CreateTicketValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(5).WithMessage("Title must be at least 5 characters")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters")
            .MaximumLength(10000).WithMessage("Description cannot exceed 10000 characters");

        RuleFor(x => x.Priority)
            .Must(BeAValidPriority).WithMessage("Priority must be one of: Low, Medium, High, Critical");
    }

    private bool BeAValidPriority(string priority)
    {
        return priority is "Low" or "Medium" or "High" or "Critical";
    }
}
