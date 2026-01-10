using Hickory.Api.Infrastructure.Data.Entities;

namespace Hickory.Api.Common;

/// <summary>
/// Constants for role-based authorization to prevent typos in [Authorize] attributes.
/// These values must match the UserRole enum values.
/// </summary>
public static class AuthorizationRoles
{
    /// <summary>
    /// End user role - can create and view their own tickets
    /// </summary>
    public const string EndUser = nameof(UserRole.EndUser);
    
    /// <summary>
    /// Support agent role - can manage assigned tickets and view all tickets
    /// </summary>
    public const string Agent = nameof(UserRole.Agent);
    
    /// <summary>
    /// Administrator role - full system access
    /// </summary>
    public const string Administrator = nameof(UserRole.Administrator);
    
    /// <summary>
    /// Combined role string for Agent or Administrator access
    /// </summary>
    public const string AgentOrAdministrator = $"{Agent},{Administrator}";
}
