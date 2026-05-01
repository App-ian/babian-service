using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;

namespace Babian.BusinessLayers.MarketConfigs.Interfaces;

public interface IMarketConfigRepository
{
    Task<MarketConfig?> GetByBarmanIdAsync(Guid barmanId, CancellationToken cancellationToken);
    Task AddAsync(MarketConfig config, CancellationToken cancellationToken);
    Task UpdateAsync(MarketConfig config, CancellationToken cancellationToken);
}
