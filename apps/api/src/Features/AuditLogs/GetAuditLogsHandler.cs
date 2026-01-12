using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.AuditLogs;

public record GetAuditLogsQuery(
    int Page = 1,
    int PageSize = 50,
    AuditAction? Action = null,
    Guid? UserId = null,
    string? EntityType = null,
    string? EntityId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<GetAuditLogsResponse>;

public record AuditLogDto(
    Guid Id,
    DateTime Timestamp,
    string Action,
    Guid? UserId,
    string? UserEmail,
    string? EntityType,
    string? EntityId,
    string? OldValues,
    string? NewValues,
    string? IpAddress,
    string? Details
);

public record GetAuditLogsResponse(
    List<AuditLogDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public class GetAuditLogsHandler : IRequestHandler<GetAuditLogsQuery, GetAuditLogsResponse>
{
    private readonly ApplicationDbContext _context;

    public GetAuditLogsHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetAuditLogsResponse> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs.AsQueryable();
        
        // Apply filters
        if (request.Action.HasValue)
        {
            query = query.Where(a => a.Action == request.Action.Value);
        }
        
        if (request.UserId.HasValue)
        {
            query = query.Where(a => a.UserId == request.UserId.Value);
        }
        
        if (!string.IsNullOrEmpty(request.EntityType))
        {
            query = query.Where(a => a.EntityType == request.EntityType);
        }
        
        if (!string.IsNullOrEmpty(request.EntityId))
        {
            query = query.Where(a => a.EntityId == request.EntityId);
        }
        
        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= request.FromDate.Value);
        }
        
        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= request.ToDate.Value);
        }
        
        // Get total count for pagination
        var totalCount = await query.CountAsync(cancellationToken);
        
        // Apply pagination and ordering (newest first)
        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogDto(
                a.Id,
                a.Timestamp,
                a.Action.ToString(),
                a.UserId,
                a.UserEmail,
                a.EntityType,
                a.EntityId,
                a.OldValues,
                a.NewValues,
                a.IpAddress,
                a.Details
            ))
            .ToListAsync(cancellationToken);
        
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
        
        return new GetAuditLogsResponse(items, totalCount, request.Page, request.PageSize, totalPages);
    }
}
