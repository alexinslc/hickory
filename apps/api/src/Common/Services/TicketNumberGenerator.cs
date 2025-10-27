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
        // Find the highest numeric ticket number
        var maxNumber = await _dbContext.Tickets
            .Where(t => t.TicketNumber.StartsWith("TKT-"))
            .Select(t => t.TicketNumber.Substring(4))
            .Where(num => int.TryParse(num, out _))
            .Select(num => int.Parse(num))
            .DefaultIfEmpty(0)
            .MaxAsync(cancellationToken);

        int nextNumber = maxNumber + 1;

        return $"TKT-{nextNumber:D5}"; // Format: TKT-00001
    }
}
