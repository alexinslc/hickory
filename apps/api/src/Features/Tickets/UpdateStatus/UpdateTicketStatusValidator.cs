using FluentValidation;
using Hickory.Api.Infrastructure.Data.Entities;

namespace Hickory.Api.Features.Tickets.UpdateStatus;

public class UpdateTicketStatusValidator : AbstractValidator<UpdateTicketStatusCommand>
{
    public UpdateTicketStatusValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty()
            .WithMessage("Ticket ID is required");

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("Invalid ticket status");

        RuleFor(x => x.NewStatus)
            .NotEqual(TicketStatus.Closed)
            .WithMessage("Use CloseTicket command to close a ticket with resolution notes");
    }
}
