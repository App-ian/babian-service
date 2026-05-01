using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;

namespace Babian.Domain.Interfaces;

public interface IDrinkRepository
{
    Task<GlobalDrink?> GetGlobalDrinkByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Drink?> GetByOwnerAndGlobalIdAsync(Guid ownerId, Guid globalDrinkId, CancellationToken cancellationToken);
    Task AddAsync(Drink drink, CancellationToken cancellationToken);
    Task UpdateAsync(Drink drink, CancellationToken cancellationToken);
    Task<Drink?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Drink>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken);
}
