using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;
using System.Collections.Generic;

namespace Babian.BusinessLayers.Drinks.Interfaces;

public interface IDrinkRepository
{
    Task<GlobalDrink?> GetGlobalDrinkByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Drink?> GetByOwnerAndGlobalIdAsync(Guid ownerId, Guid globalDrinkId, CancellationToken cancellationToken);
    Task AddAsync(Drink drink, CancellationToken cancellationToken);
    Task UpdateAsync(Drink drink, CancellationToken cancellationToken);
    Task<List<Drink>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken);
}
