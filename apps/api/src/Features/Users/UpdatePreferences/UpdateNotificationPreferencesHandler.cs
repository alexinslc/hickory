using Hickory.Api.Infrastructure.Data;
using Hickory.Api.Infrastructure.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Features.Users.UpdatePreferences;

public record UpdateNotificationPreferencesCommand(
    Guid UserId,
    UpdateNotificationPreferencesRequest Request
) : IRequest<NotificationPreferencesResponse>;

public record UpdateNotificationPreferencesRequest
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
    public string? WebhookSecret { get; init; }
}

public record NotificationPreferencesResponse
{
    public bool EmailEnabled { get; init; }
    public bool InAppEnabled { get; init; }
    public bool WebhookEnabled { get; init; }
    public string? WebhookUrl { get; init; }
}

public class UpdateNotificationPreferencesHandler : IRequestHandler<UpdateNotificationPreferencesCommand, NotificationPreferencesResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateNotificationPreferencesHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NotificationPreferencesResponse> Handle(UpdateNotificationPreferencesCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        
        var prefs = await _dbContext.NotificationPreferences
            .Where(np => np.UserId == command.UserId)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (prefs == null)
        {
            // Create new preferences
            prefs = new NotificationPreferences
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                EmailEnabled = request.EmailEnabled,
                EmailOnTicketCreated = request.EmailOnTicketCreated,
                EmailOnTicketUpdated = request.EmailOnTicketUpdated,
                EmailOnTicketAssigned = request.EmailOnTicketAssigned,
                EmailOnCommentAdded = request.EmailOnCommentAdded,
                InAppEnabled = request.InAppEnabled,
                InAppOnTicketCreated = request.InAppOnTicketCreated,
                InAppOnTicketUpdated = request.InAppOnTicketUpdated,
                InAppOnTicketAssigned = request.InAppOnTicketAssigned,
                InAppOnCommentAdded = request.InAppOnCommentAdded,
                WebhookEnabled = request.WebhookEnabled,
                WebhookUrl = request.WebhookUrl,
                WebhookSecret = request.WebhookSecret
            };
            
            _dbContext.NotificationPreferences.Add(prefs);
        }
        else
        {
            // Update existing preferences
            prefs.EmailEnabled = request.EmailEnabled;
            prefs.EmailOnTicketCreated = request.EmailOnTicketCreated;
            prefs.EmailOnTicketUpdated = request.EmailOnTicketUpdated;
            prefs.EmailOnTicketAssigned = request.EmailOnTicketAssigned;
            prefs.EmailOnCommentAdded = request.EmailOnCommentAdded;
            prefs.InAppEnabled = request.InAppEnabled;
            prefs.InAppOnTicketCreated = request.InAppOnTicketCreated;
            prefs.InAppOnTicketUpdated = request.InAppOnTicketUpdated;
            prefs.InAppOnTicketAssigned = request.InAppOnTicketAssigned;
            prefs.InAppOnCommentAdded = request.InAppOnCommentAdded;
            prefs.WebhookEnabled = request.WebhookEnabled;
            prefs.WebhookUrl = request.WebhookUrl;
            prefs.WebhookSecret = request.WebhookSecret;
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return new NotificationPreferencesResponse
        {
            EmailEnabled = prefs.EmailEnabled,
            InAppEnabled = prefs.InAppEnabled,
            WebhookEnabled = prefs.WebhookEnabled,
            WebhookUrl = prefs.WebhookUrl
        };
    }
}
