using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;

namespace Babian.Domain.Interfaces;

public interface IPriceHistoryRepository
{
    Task AddAsync(PriceHistory history, CancellationToken cancellationToken);
    Task<Dictionary<Guid, decimal>> GetLastVariationsBySessionAsync(Guid sessionId, CancellationToken cancellationToken);
}
