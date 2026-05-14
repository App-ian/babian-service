using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;
using Babian.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Babian.Infrastructure.Persistence.Repositories;

public class MarketEventRepository : IMarketEventRepository
{
    private readonly AppDbContext _context;

    public MarketEventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<MarketEvent>> GetActiveEventsByBarmanIdAsync(Guid barmanId, CancellationToken cancellationToken)
    {
        // On récupère la session active pour filtrer strictement
        var activeSession = await _context.MarketSessions
            .Where(s => s.OwnerId == barmanId && s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeSession == null) return new List<MarketEvent>();

        var now = DateTime.UtcNow;
        return await _context.MarketEvents
            .Where(x => x.BarmanId == barmanId && 
                        x.MarketSessionId == activeSession.Id &&
                        (x.Status == MarketEventStatus.Active || x.Status == MarketEventStatus.Scheduled) && 
                        x.StartAt <= now && x.EndAt >= now)
            .ToListAsync(cancellationToken);
    }

    public async Task<MarketEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.MarketEvents.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(MarketEvent marketEvent, CancellationToken cancellationToken)
    {
        _context.MarketEvents.Add(marketEvent);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(MarketEvent marketEvent, CancellationToken cancellationToken)
    {
        _context.MarketEvents.Update(marketEvent);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            _context.Entry(marketEvent).State = EntityState.Detached;
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var marketEvent = await GetByIdAsync(id, cancellationToken);
        if (marketEvent != null)
        {
            _context.MarketEvents.Remove(marketEvent);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<MarketEvent>> GetByBarmanIdAsync(Guid barmanId, CancellationToken cancellationToken)
    {
        return await _context.MarketEvents
            .AsNoTracking()
            .Where(x => x.BarmanId == barmanId)
            .OrderByDescending(x => x.StartAt)
            .ToListAsync(cancellationToken);
    }
}
