using Hickory.Api.Common;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hickory.Api.Features.AuditLogs;

[ApiController]
[Route("api/v1/audit-logs")]
[Authorize(Roles = AuthorizationRoles.Administrator)]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get audit logs with optional filtering (Admin only)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetAuditLogsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetAuditLogsResponse>> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] AuditAction? action = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? entityId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        // Clamp page size
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        
        var query = new GetAuditLogsQuery(
            page, pageSize, action, userId, entityType, entityId, fromDate, toDate);
        
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get available audit action types for filtering
    /// </summary>
    [HttpGet("actions")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<string>> GetAuditActions()
    {
        var actions = Enum.GetNames<AuditAction>();
        return Ok(actions);
    }
}
