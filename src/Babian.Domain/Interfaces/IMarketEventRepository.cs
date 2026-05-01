using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;

namespace Babian.Domain.Interfaces;

public interface IMarketEventRepository
{
    Task<List<MarketEvent>> GetActiveEventsByBarmanIdAsync(Guid barmanId, CancellationToken cancellationToken);
    Task<MarketEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(MarketEvent marketEvent, CancellationToken cancellationToken);
    Task UpdateAsync(MarketEvent marketEvent, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<List<MarketEvent>> GetByBarmanIdAsync(Guid barmanId, CancellationToken cancellationToken);
}
