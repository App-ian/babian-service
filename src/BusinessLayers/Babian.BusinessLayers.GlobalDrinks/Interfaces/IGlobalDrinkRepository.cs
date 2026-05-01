using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;

namespace Babian.BusinessLayers.GlobalDrinks.Interfaces;

public interface IGlobalDrinkRepository
{
    Task<List<GlobalDrink>> GetAllAsync(CancellationToken cancellationToken);
}
