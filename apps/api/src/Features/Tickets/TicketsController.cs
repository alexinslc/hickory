using Hickory.Api.Features.Tickets.Create;
using Hickory.Api.Features.Tickets.Create.Models;
using Hickory.Api.Features.Tickets.GetById;
using Hickory.Api.Features.Tickets.GetBySubmitter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hickory.Api.Features.Tickets;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TicketsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<CreateTicketResponse>> CreateTicket(
        [FromBody] CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var command = new CreateTicketCommand(request, userId);
        var response = await _mediator.Send(command, cancellationToken);
        
        return CreatedAtAction(
            nameof(GetTicketById), 
            new { id = response.Id }, 
            response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Models.TicketDto>> GetTicketById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var userRole = GetUserRole();
        var query = new GetTicketByIdQuery(id, userId, userRole);
        var ticket = await _mediator.Send(query, cancellationToken);

        if (ticket == null)
        {
            return NotFound();
        }

        return Ok(ticket);
    }

    [HttpGet]
    public async Task<ActionResult<List<Models.TicketDto>>> GetMyTickets(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var query = new GetTicketsBySubmitterQuery(userId);
        var tickets = await _mediator.Send(query, cancellationToken);

        return Ok(tickets);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims");
        }

        return userId;
    }

    private string GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value 
            ?? User.FindFirst("role")?.Value 
            ?? "User";
    }
}
