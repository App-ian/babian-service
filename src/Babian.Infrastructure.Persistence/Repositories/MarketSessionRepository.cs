using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using Babian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Babian.Infrastructure.Persistence.Repositories;

public class MarketSessionRepository : IMarketSessionRepository
{
    private readonly AppDbContext _context;

    public MarketSessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MarketSession?> GetActiveSessionAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        return await _context.MarketSessions
            .Where(x => x.OwnerId == ownerId && x.IsActive)
            .OrderByDescending(x => x.StartTime)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<MarketSession> CreateAsync(MarketSession session, CancellationToken cancellationToken)
    {
        _context.MarketSessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task UpdateAsync(MarketSession session, CancellationToken cancellationToken)
    {
        _context.MarketSessions.Update(session);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
