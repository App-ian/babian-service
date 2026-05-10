using System;
using System.Collections.Generic;
using System.Linq;
using Babian.Domain.Entities;
using Babian.Domain.Interfaces;

namespace Babian.BusinessLayers.MarketEngine.Services;

public class PriceCalculationService : IPriceCalculationService
{
    private readonly IPriceCalculator _priceCalculator;

    public PriceCalculationService(IPriceCalculator priceCalculator)
    {
        _priceCalculator = priceCalculator;
    }

    public decimal CalculatePureMarketPrice(Drink drink, int rank, int sales, List<RankingGroup>? rankingGroups)
    {
        decimal coefficient;
        
        // Gérer le fallback si rankingGroups est null ou vide
        if (rankingGroups != null && rankingGroups.Count > 0)
        {
            var sortedGroups = rankingGroups.OrderBy(g => g.MaxRank).ToList();
            var group = sortedGroups.FirstOrDefault(g => rank <= g.MaxRank) ?? sortedGroups.Last();
            coefficient = group.Coefficient;
        }
        else
        {
            // Aucun groupe configuré : fallback binaire (comme dans le code d'origine)
            coefficient = sales > 0 ? 0.15m : -0.15m;
        }

        // Déterminer le prix cible et la vitesse depuis le coefficient
        decimal targetPrice;
        decimal speed;

        if (coefficient > 0)
        {
            targetPrice = drink.MaxPrice;
            speed = coefficient;
        }
        else if (coefficient < 0)
        {
            targetPrice = drink.MinPrice;
            speed = Math.Abs(coefficient);
        }
        else
        {
            // coefficient == 0 → prix stable
            return drink.MarketPrice;
        }

        // Calcul asymptotique
        return _priceCalculator.CalculateNextPrice(drink.MarketPrice, drink.MinPrice, drink.MaxPrice, targetPrice, speed);
    }
}
