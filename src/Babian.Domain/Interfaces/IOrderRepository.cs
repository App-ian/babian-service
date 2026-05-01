using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;

namespace Babian.Domain.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);
    Task<int> GetQuantityByDrinkAndSessionAsync(Guid drinkId, Guid sessionId, CancellationToken cancellationToken);
    Task<Dictionary<Guid, int>> GetCountsBySessionAsync(Guid sessionId, int? cycleNumber, DateTime? since, CancellationToken cancellationToken);
}
