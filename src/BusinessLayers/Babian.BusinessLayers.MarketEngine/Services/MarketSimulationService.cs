using System;
using System.Collections.Generic;
using System.Linq;
using Babian.Domain.Entities;
using Babian.Domain.Interfaces;

namespace Babian.BusinessLayers.MarketEngine.Services;

public class MarketSimulationService : IMarketSimulationService
{
    private readonly IPriceCalculator _priceCalculator;

    public MarketSimulationService(IPriceCalculator priceCalculator)
    {
        _priceCalculator = priceCalculator;
    }

    public decimal SimulateNextPrice(decimal currentPrice, decimal minPrice, decimal maxPrice, int rank, List<RankingGroup> groups)
    {
        // On s'assure d'avoir les groupes triés
        var sortedGroups = groups?.OrderBy(g => g.MaxRank).ToList();

        decimal coefficient;
        if (sortedGroups != null && sortedGroups.Any())
        {
            var group = sortedGroups.FirstOrDefault(g => rank <= g.MaxRank) 
                        ?? sortedGroups.Last();
            coefficient = group.Coefficient;
        }
        else
        {
            // Fallback binaire simple si pas de groupes (cohérent avec UpdateMarketPricesCommandHandler)
            coefficient = rank <= 15 ? 0.15m : -0.15m;
        }

        return CalculateWithCoefficient(currentPrice, minPrice, maxPrice, coefficient);
    }

    private decimal CalculateWithCoefficient(decimal currentPrice, decimal minPrice, decimal maxPrice, decimal coefficient)
    {
        decimal targetPrice;
        decimal speed;

        if (coefficient > 0)
        {
            targetPrice = maxPrice;
            speed = coefficient;
        }
        else if (coefficient < 0)
        {
            targetPrice = minPrice;
            speed = Math.Abs(coefficient);
        }
        else
        {
            return currentPrice;
        }

        return _priceCalculator.CalculateNextPrice(currentPrice, minPrice, maxPrice, targetPrice, speed);
    }
}
