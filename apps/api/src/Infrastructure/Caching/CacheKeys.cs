namespace Hickory.Api.Infrastructure.Caching;

/// <summary>
/// Helper class for generating consistent cache keys
/// </summary>
public static class CacheKeys
{
    private const string Prefix = "hickory";
    
    // Ticket cache keys
    public static string Ticket(Guid ticketId) => $"{Prefix}:ticket:{ticketId}";
    public static string TicketsBySubmitter(Guid submitterId, int page, int pageSize) => 
        $"{Prefix}:tickets:submitter:{submitterId}:page:{page}:size:{pageSize}";
    public static string TicketsByStatus(string status, int page, int pageSize) => 
        $"{Prefix}:tickets:status:{status}:page:{page}:size:{pageSize}";
    public static string AgentQueue(Guid agentId) => $"{Prefix}:tickets:queue:agent:{agentId}";
    public static string TicketsPattern(Guid? ticketId = null) => 
        ticketId.HasValue ? Ticket(ticketId.Value) : $"{Prefix}:ticket:*";
    public static string AllTicketsPattern() => $"{Prefix}:tickets:*";
    
    // Knowledge base cache keys
    public static string Article(Guid articleId) => $"{Prefix}:article:{articleId}";
    public static string ArticlesByCategory(Guid categoryId, int page, int pageSize) => 
        $"{Prefix}:articles:category:{categoryId}:page:{page}:size:{pageSize}";
    public static string SuggestedArticles(string searchText) => 
        $"{Prefix}:articles:suggested:{searchText}";
    public static string ArticlesPattern(Guid? articleId = null) => 
        articleId.HasValue ? Article(articleId.Value) : $"{Prefix}:article:*";
    public static string AllArticlesPattern() => $"{Prefix}:articles:*";
    
    // User cache keys
    public static string User(Guid userId) => $"{Prefix}:user:{userId}";
    public static string UserByEmail(string email) => $"{Prefix}:user:email:{email}";
    public static string UsersPattern() => $"{Prefix}:user:*";
}

/// <summary>
/// Default cache expiration times
/// </summary>
public static class CacheExpiration
{
    public static readonly TimeSpan Tickets = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan TicketDetails = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan KnowledgeArticles = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan UserProfiles = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan SearchResults = TimeSpan.FromMinutes(10);
}
