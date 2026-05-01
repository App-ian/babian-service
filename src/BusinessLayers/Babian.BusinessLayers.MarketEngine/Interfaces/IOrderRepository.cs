using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;

namespace Babian.BusinessLayers.MarketEngine.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);
    Task<int> GetQuantityByDrinkAndSessionAsync(Guid drinkId, Guid sessionId, CancellationToken cancellationToken);
}
