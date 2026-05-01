using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;

namespace Babian.BusinessLayers.MarketEngine.Interfaces;

public interface IPriceHistoryRepository
{
    Task AddAsync(PriceHistory history, CancellationToken cancellationToken);
}
