using Hickory.Api.Features.Tickets.AddTags;
using Hickory.Api.Features.Tickets.Assign;
using Hickory.Api.Features.Tickets.Close;
using Hickory.Api.Features.Tickets.Create;
using Hickory.Api.Features.Tickets.Create.Models;
using Hickory.Api.Features.Tickets.GetById;
using Hickory.Api.Features.Tickets.GetBySubmitter;
using Hickory.Api.Features.Tickets.GetQueue;
using Hickory.Api.Features.Tickets.Reassign;
using Hickory.Api.Features.Tickets.RemoveTags;
using Hickory.Api.Features.Tickets.UpdatePriority;
using Hickory.Api.Features.Tickets.UpdateStatus;
using Hickory.Api.Infrastructure.Data.Entities;
using Hickory.Api.Common;
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

    [HttpGet("{id}/details")]
    public async Task<ActionResult<TicketDetailsResponse>> GetTicketDetails(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var userRole = GetUserRole();
        var query = new GetTicketDetailsQuery(id, userId, userRole);
        var details = await _mediator.Send(query, cancellationToken);

        if (details == null)
        {
            return NotFound();
        }

        return Ok(details);
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

    // Agent-only endpoints
    [HttpGet("queue")]
    [Authorize(Roles = AuthorizationRoles.AgentOrAdministrator)]
    public async Task<ActionResult<List<Models.TicketDto>>> GetAgentQueue(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var query = new GetAgentQueueQuery(userId);
        var tickets = await _mediator.Send(query, cancellationToken);

        return Ok(tickets);
    }

    [HttpPut("{id}/assign")]
    [Authorize(Roles = AuthorizationRoles.AgentOrAdministrator)]
    public async Task<IActionResult> AssignTicket(
        Guid id,
        [FromBody] AssignTicketRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignTicketCommand(id, request.AgentId);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = AuthorizationRoles.AgentOrAdministrator)]
    public async Task<IActionResult> UpdateTicketStatus(
        Guid id,
        [FromBody] UpdateTicketStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTicketStatusCommand(id, request.NewStatus);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpPut("{id}/priority")]
    [Authorize(Roles = AuthorizationRoles.AgentOrAdministrator)]
    public async Task<IActionResult> UpdateTicketPriority(
        Guid id,
        [FromBody] UpdateTicketPriorityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTicketPriorityCommand(id, request.NewPriority);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpPost("{id}/close")]
    [Authorize(Roles = AuthorizationRoles.AgentOrAdministrator)]
    public async Task<IActionResult> CloseTicket(
        Guid id,
        [FromBody] CloseTicketRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CloseTicketCommand(id, request.ResolutionNotes);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpPut("{id}/reassign")]
    [Authorize(Roles = AuthorizationRoles.AgentOrAdministrator)]
    public async Task<IActionResult> ReassignTicket(
        Guid id,
        [FromBody] ReassignTicketRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReassignTicketCommand(id, request.NewAgentId);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpPost("{id}/tags")]
    public async Task<IActionResult> AddTags(
        Guid id,
        [FromBody] List<string> tags,
        CancellationToken cancellationToken)
    {
        var command = new AddTagsToTicketCommand(id, tags);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id}/tags")]
    public async Task<IActionResult> RemoveTags(
        Guid id,
        [FromBody] List<string> tags,
        CancellationToken cancellationToken)
    {
        var command = new RemoveTagsFromTicketCommand(id, tags);
        await _mediator.Send(command, cancellationToken);

        return NoContent();
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
