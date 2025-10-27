using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hickory.Api.Features.Search;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;

    public SearchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search tickets using full-text search with filters
    /// </summary>
    /// <param name="q">Search query (minimum 2 characters)</param>
    /// <param name="status">Filter by ticket status</param>
    /// <param name="priority">Filter by ticket priority</param>
    /// <param name="assignedToId">Filter by assigned agent ID</param>
    /// <param name="createdAfter">Filter by creation date (inclusive)</param>
    /// <param name="createdBefore">Filter by creation date (inclusive)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Results per page (default: 20, max: 100)</param>
    /// <returns>Paginated search results</returns>
    [HttpGet]
    public async Task<ActionResult<SearchTicketsResult>> SearchTickets(
        [FromQuery] string? q,
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketPriority? priority,
        [FromQuery] Guid? assignedToId,
        [FromQuery] DateTime? createdAfter,
        [FromQuery] DateTime? createdBefore,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var userRole = GetUserRole();

        var query = new SearchTicketsQuery(
            q,
            userId,
            userRole,
            status,
            priority,
            assignedToId,
            createdAfter,
            createdBefore,
            page,
            pageSize
        );

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
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
