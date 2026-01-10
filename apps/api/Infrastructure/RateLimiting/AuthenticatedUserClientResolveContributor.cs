using AspNetCoreRateLimit;

namespace Hickory.Api.Infrastructure.RateLimiting;

/// <summary>
/// Custom client resolver that identifies authenticated users by their user ID from JWT claims.
/// This allows authenticated users to have different (more lenient) rate limits than anonymous users.
/// </summary>
public class AuthenticatedUserClientResolveContributor : IClientResolveContributor
{
    public Task<string> ResolveClientAsync(HttpContext httpContext)
    {
        // Check if user is authenticated
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            // Get user ID from JWT claims (sub claim)
            var userId = httpContext.User.FindFirst("sub")?.Value 
                ?? httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Return authenticated user identifier for client rate limiting
                return Task.FromResult($"authenticated-user:{userId}");
            }
        }
        
        // Return empty string for anonymous users (falls back to IP-based rate limiting)
        return Task.FromResult(string.Empty);
    }
}
