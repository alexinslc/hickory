using System.Security.Claims;
using Hickory.Api.Infrastructure.Audit;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;

namespace Hickory.Api.Infrastructure.Behaviors;

/// <summary>
/// Marker interface for commands that should be audited
/// </summary>
public interface IAuditableRequest
{
    /// <summary>
    /// The type of entity being affected
    /// </summary>
    string EntityType { get; }
    
    /// <summary>
    /// The ID of the entity being affected (if known)
    /// </summary>
    string? EntityId { get; }
}

/// <summary>
/// Marker interface for commands that create entities
/// </summary>
public interface IAuditableCreateRequest : IAuditableRequest { }

/// <summary>
/// Marker interface for commands that update entities
/// </summary>
public interface IAuditableUpdateRequest : IAuditableRequest { }

/// <summary>
/// Marker interface for commands that delete entities
/// </summary>
public interface IAuditableDeleteRequest : IAuditableRequest { }

/// <summary>
/// MediatR pipeline behavior that automatically audits commands marked with IAuditableRequest
/// </summary>
public class AuditingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditingBehavior<TRequest, TResponse>> _logger;

    public AuditingBehavior(
        IAuditLogService auditLogService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditingBehavior<TRequest, TResponse>> logger)
    {
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only audit requests that implement IAuditableRequest
        if (request is not IAuditableRequest auditableRequest)
        {
            return await next();
        }

        var response = await next();
        
        // Determine the action based on the interface
        var action = request switch
        {
            IAuditableCreateRequest => AuditAction.Create,
            IAuditableUpdateRequest => AuditAction.Update,
            IAuditableDeleteRequest => AuditAction.Delete,
            _ => AuditAction.Update // Default to update for general auditable requests
        };
        
        // Get current user info from HttpContext
        var (userId, userEmail) = GetCurrentUser();
        
        // Get the entity ID - could be in the request or in the response
        var entityId = auditableRequest.EntityId ?? GetEntityIdFromResponse(response);
        
        // Log the audit event (fire and forget - don't block the response)
        _ = _auditLogService.LogAsync(
            action,
            userId: userId,
            userEmail: userEmail,
            entityType: auditableRequest.EntityType,
            entityId: entityId,
            newValues: request, // Log the request as the new values
            cancellationToken: cancellationToken);

        return response;
    }

    private (Guid? UserId, string? Email) GetCurrentUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return (null, null);
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = user.FindFirst(ClaimTypes.Email)?.Value;
        
        Guid? userId = Guid.TryParse(userIdClaim, out var id) ? id : null;
        
        return (userId, email);
    }

    private static string? GetEntityIdFromResponse(TResponse response)
    {
        // Try to extract ID from common response patterns
        return response switch
        {
            Guid guidId => guidId.ToString(),
            { } obj when obj.GetType().GetProperty("Id") is { } prop => 
                prop.GetValue(obj)?.ToString(),
            _ => null
        };
    }
}
