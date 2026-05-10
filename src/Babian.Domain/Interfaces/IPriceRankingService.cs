using System;
using System.Collections.Generic;
using Babian.Domain.Entities;

namespace Babian.Domain.Interfaces;

public record DrinkRankResult(Drink Drink, int Rank, int Sales);

public interface IPriceRankingService
{
    List<DrinkRankResult> RankDrinks(IEnumerable<Drink> drinks, Dictionary<Guid, int> cycleCounts);
}
