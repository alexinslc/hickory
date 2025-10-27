namespace Hickory.Api.Infrastructure.Data.Entities;

/// <summary>
/// User roles for role-based authorization
/// </summary>
public enum UserRole
{
    /// <summary>
    /// End user who can create and view their own tickets
    /// </summary>
    EndUser = 0,
    
    /// <summary>
    /// Support agent who can manage assigned tickets and view all tickets
    /// </summary>
    Agent = 1,
    
    /// <summary>
    /// Administrator with full system access
    /// </summary>
    Administrator = 2
}
