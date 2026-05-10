using System.Collections.Generic;
using Babian.Domain.Entities;

namespace Babian.Domain.Interfaces;

public interface IPriceCalculationService
{
    decimal CalculatePureMarketPrice(Drink drink, int rank, int sales, List<RankingGroup>? rankingGroups);
}
