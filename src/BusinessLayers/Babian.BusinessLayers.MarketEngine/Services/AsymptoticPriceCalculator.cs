using System;
using Babian.Domain.Interfaces;

namespace Babian.BusinessLayers.MarketEngine.Services;

public class AsymptoticPriceCalculator : IPriceCalculator
{
    public decimal CalculateNextPrice(decimal currentPrice, decimal minPrice, decimal maxPrice, decimal targetPrice, decimal speed)
    {
        // Formule Asymptotique : Nouveau = Actuel + (Cible - Actuel) * Vitesse
        // La vitesse (speed) doit être comprise entre 0 et 1 (typiquement 0.1 pour 10% de la distance par cycle)
        
        decimal variation = (targetPrice - currentPrice) * speed;
        decimal nextPrice = currentPrice + variation;

        // On s'assure de ne jamais dépasser les bornes
        if (nextPrice < minPrice) return minPrice;
        if (nextPrice > maxPrice) return maxPrice;

        return Math.Round(nextPrice, 2); // On arrondit au centime
    }
}
