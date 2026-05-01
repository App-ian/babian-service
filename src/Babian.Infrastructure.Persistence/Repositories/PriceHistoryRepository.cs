using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Babian.Domain.Interfaces;
using Babian.Domain.Entities;

namespace Babian.Infrastructure.Persistence.Repositories;

public class PriceHistoryRepository : IPriceHistoryRepository
{
    private readonly AppDbContext _context;

    public PriceHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PriceHistory history, CancellationToken cancellationToken)
    {
        _context.PriceHistories.Add(history);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, decimal>> GetLastVariationsBySessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        // On récupère le dernier cycle pour cette session
        var lastEntries = await _context.PriceHistories
            .Where(x => x.MarketSessionId == sessionId)
            .GroupBy(x => x.DrinkId)
            .Select(g => g.OrderByDescending(x => x.CreatedAt).First())
            .ToListAsync(cancellationToken);

        return lastEntries.ToDictionary(x => x.DrinkId, x => x.Variation);
    }
}
