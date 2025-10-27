using FluentValidation;

namespace Hickory.Api.Features.Tickets.Close;

public class CloseTicketValidator : AbstractValidator<CloseTicketCommand>
{
    public CloseTicketValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty()
            .WithMessage("Ticket ID is required");

        RuleFor(x => x.ResolutionNotes)
            .NotEmpty()
            .WithMessage("Resolution notes are required when closing a ticket")
            .MinimumLength(10)
            .WithMessage("Resolution notes must be at least 10 characters")
            .MaximumLength(5000)
            .WithMessage("Resolution notes cannot exceed 5000 characters");
    }
}
