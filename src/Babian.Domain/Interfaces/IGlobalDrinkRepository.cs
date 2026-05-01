using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;

namespace Babian.Domain.Interfaces;

public interface IGlobalDrinkRepository
{
    Task<List<GlobalDrink>> GetAllAsync(CancellationToken cancellationToken);
    Task<GlobalDrink?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(GlobalDrink globalDrink, CancellationToken cancellationToken);
    Task UpdateAsync(GlobalDrink globalDrink, CancellationToken cancellationToken);
    Task DeleteAsync(GlobalDrink globalDrink, CancellationToken cancellationToken);
}
