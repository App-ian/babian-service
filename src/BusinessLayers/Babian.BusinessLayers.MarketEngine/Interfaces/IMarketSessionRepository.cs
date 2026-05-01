using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;

namespace Babian.BusinessLayers.MarketEngine.Interfaces;

public interface IMarketSessionRepository
{
    Task<MarketSession?> GetActiveSessionAsync(Guid ownerId, CancellationToken cancellationToken);
    Task<MarketSession> CreateAsync(MarketSession session, CancellationToken cancellationToken);
    Task UpdateAsync(MarketSession session, CancellationToken cancellationToken);
}
