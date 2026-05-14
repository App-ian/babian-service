using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using Babian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Babian.Infrastructure.Persistence.Repositories;

public class MarketConfigRepository : IMarketConfigRepository
{
    private readonly AppDbContext _context;

    public MarketConfigRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<MarketConfig?> GetByBarmanIdAsync(Guid barmanId, CancellationToken cancellationToken)
    {
        return await _context.MarketConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BarmanId == barmanId, cancellationToken);
    }

    public async Task AddAsync(MarketConfig config, CancellationToken cancellationToken)
    {
        await _context.MarketConfigs.AddAsync(config, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(MarketConfig config, CancellationToken cancellationToken)
    {
        _context.MarketConfigs.Update(config);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            _context.Entry(config).State = EntityState.Detached;
            throw;
        }
    }
}
