namespace Hickory.Api.Features.Tickets.Close;

public record CloseTicketRequest
{
    public string ResolutionNotes { get; init; } = string.Empty;
}
