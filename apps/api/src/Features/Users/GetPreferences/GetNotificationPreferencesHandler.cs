using Hickory.Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Users.GetPreferences;

public record GetNotificationPreferencesQuery(Guid UserId) : IRequest<NotificationPreferencesDto>;

public record NotificationPreferencesDto
{
    public bool EmailEnabled { get; init; }
    public bool EmailOnTicketCreated { get; init; }
    public bool EmailOnTicketUpdated { get; init; }
    public bool EmailOnTicketAssigned { get; init; }
    public bool EmailOnCommentAdded { get; init; }
    
    public bool InAppEnabled { get; init; }
    public bool InAppOnTicketCreated { get; init; }
    public bool InAppOnTicketUpdated { get; init; }
    public bool InAppOnTicketAssigned { get; init; }
    public bool InAppOnCommentAdded { get; init; }
    
    public bool WebhookEnabled { get; init; }
    public string? WebhookUrl { get; init; }
}

public class GetNotificationPreferencesHandler : IRequestHandler<GetNotificationPreferencesQuery, NotificationPreferencesDto>
{
    private readonly ApplicationDbContext _dbContext;

    public GetNotificationPreferencesHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NotificationPreferencesDto> Handle(GetNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var prefs = await _dbContext.NotificationPreferences
            .Where(np => np.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);
        
        // Return default preferences if none exist
        if (prefs == null)
        {
            return new NotificationPreferencesDto
            {
                EmailEnabled = true,
                EmailOnTicketCreated = true,
                EmailOnTicketUpdated = true,
                EmailOnTicketAssigned = true,
                EmailOnCommentAdded = true,
                InAppEnabled = true,
                InAppOnTicketCreated = true,
                InAppOnTicketUpdated = true,
                InAppOnTicketAssigned = true,
                InAppOnCommentAdded = true,
                WebhookEnabled = false,
                WebhookUrl = null
            };
        }
        
        return new NotificationPreferencesDto
        {
            EmailEnabled = prefs.EmailEnabled,
            EmailOnTicketCreated = prefs.EmailOnTicketCreated,
            EmailOnTicketUpdated = prefs.EmailOnTicketUpdated,
            EmailOnTicketAssigned = prefs.EmailOnTicketAssigned,
            EmailOnCommentAdded = prefs.EmailOnCommentAdded,
            InAppEnabled = prefs.InAppEnabled,
            InAppOnTicketCreated = prefs.InAppOnTicketCreated,
            InAppOnTicketUpdated = prefs.InAppOnTicketUpdated,
            InAppOnTicketAssigned = prefs.InAppOnTicketAssigned,
            InAppOnCommentAdded = prefs.InAppOnCommentAdded,
            WebhookEnabled = prefs.WebhookEnabled,
            WebhookUrl = prefs.WebhookUrl
        };
    }
}
