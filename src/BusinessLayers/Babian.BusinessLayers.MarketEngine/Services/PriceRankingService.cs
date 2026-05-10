using System;
using System.Collections.Generic;
using System.Linq;
using Babian.Domain.Entities;
using Babian.Domain.Interfaces;

namespace Babian.BusinessLayers.MarketEngine.Services;

public class PriceRankingService : IPriceRankingService
{
    public List<DrinkRankResult> RankDrinks(IEnumerable<Drink> drinks, Dictionary<Guid, int> cycleCounts)
    {
        var ranked = drinks
            .Select(d => new
            {
                Drink = d,
                Sales = cycleCounts != null && cycleCounts.TryGetValue(d.Id, out var sales) ? sales : 0
            })
            .OrderByDescending(x => x.Sales)
            .ThenBy(x => Guid.NewGuid())
            .ToList();

        var result = new List<DrinkRankResult>();
        for (int i = 0; i < ranked.Count; i++)
        {
            result.Add(new DrinkRankResult(ranked[i].Drink, i + 1, ranked[i].Sales));
        }

        return result;
    }
}
