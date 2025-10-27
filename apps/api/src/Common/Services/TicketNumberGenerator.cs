using Hickory.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hickory.Api.Common.Services;

public interface ITicketNumberGenerator
{
    Task<string> GenerateTicketNumberAsync(CancellationToken cancellationToken = default);
}

public class TicketNumberGenerator : ITicketNumberGenerator
{
    private readonly ApplicationDbContext _dbContext;

    public TicketNumberGenerator(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateTicketNumberAsync(CancellationToken cancellationToken = default)
    {
        // Find the highest ticket number
        // Get all ticket numbers with the TKT- prefix and parse them in memory
        var ticketNumbers = await _dbContext.Tickets
            .Where(t => t.TicketNumber.StartsWith("TKT-"))
            .Select(t => t.TicketNumber.Substring(4))
            .ToListAsync(cancellationToken);

        var maxNumber = ticketNumbers
            .Where(num => int.TryParse(num, out _))
            .Select(num => int.Parse(num))
            .DefaultIfEmpty(0)
            .Max();

        int nextNumber = maxNumber + 1;

        return $"TKT-{nextNumber:D5}"; // Format: TKT-00001
    }
}
