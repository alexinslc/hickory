using Hickory.Api.Infrastructure.Data.Entities;

namespace Hickory.Api.Features.Tickets.Create.Models;

public record CreateTicketRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Priority { get; init; } = "Medium";
    public Guid? CategoryId { get; init; }
}

public record CreateTicketResponse
{
    public Guid Id { get; init; }
    public string TicketNumber { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
