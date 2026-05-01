using System.Collections.Generic;
using Babian.Domain.Entities;

namespace Babian.Domain.Interfaces;

public interface IMarketSimulationService
{
    /// <summary>
    /// Simule le calcul du prochain prix basé sur le rang et les groupes de classement.
    /// </summary>
    decimal SimulateNextPrice(decimal currentPrice, decimal minPrice, decimal maxPrice, int rank, List<RankingGroup> groups);
}
