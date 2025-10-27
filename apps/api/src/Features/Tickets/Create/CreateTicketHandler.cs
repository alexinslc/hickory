using Hickory.Api.Common.Services;
using Hickory.Api.Features.Tickets.Create.Models;
using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Hickory.Api.Features.Tickets.Create;

public record CreateTicketCommand(CreateTicketRequest Request, Guid UserId) : IRequest<CreateTicketResponse>;

public class CreateTicketHandler : IRequestHandler<CreateTicketCommand, CreateTicketResponse>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITicketNumberGenerator _ticketNumberGenerator;

    public CreateTicketHandler(
        ApplicationDbContext dbContext,
        ITicketNumberGenerator ticketNumberGenerator)
    {
        _dbContext = dbContext;
        _ticketNumberGenerator = ticketNumberGenerator;
    }

    public async Task<CreateTicketResponse> Handle(CreateTicketCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        
        // Generate ticket number
        var ticketNumber = await _ticketNumberGenerator.GenerateTicketNumberAsync(cancellationToken);
        
        // Parse priority
        if (!Enum.TryParse<TicketPriority>(request.Priority, out var priority))
        {
            priority = TicketPriority.Medium;
        }
        
        // Create ticket
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            TicketNumber = ticketNumber,
            Title = request.Title,
            Description = request.Description,
            Status = TicketStatus.Open,
            Priority = priority,
            SubmitterId = command.UserId,
            CategoryId = request.CategoryId
        };
        
        _dbContext.Tickets.Add(ticket);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return new CreateTicketResponse
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status.ToString(),
            Priority = ticket.Priority.ToString(),
            CreatedAt = ticket.CreatedAt
        };
    }
}
