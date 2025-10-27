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
        var lastTicketNumber = await _dbContext.Tickets
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => t.TicketNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int nextNumber = 1;
        
        if (!string.IsNullOrEmpty(lastTicketNumber) && lastTicketNumber.StartsWith("TKT-"))
        {
            var numberPart = lastTicketNumber.Substring(4);
            if (int.TryParse(numberPart, out var currentNumber))
            {
                nextNumber = currentNumber + 1;
            }
        }

        return $"TKT-{nextNumber:D5}"; // Format: TKT-00001
    }
}
