using System;
using System.Collections.Generic;
using System.Linq;
using Babian.Domain.Entities;
using Babian.Domain.Interfaces;

namespace Babian.BusinessLayers.MarketEngine.Services;

public class MarketEventApplier : IMarketEventApplier
{
    public decimal ApplyEvents(Drink drink, decimal pureMarketPrice, Guid currentSessionId, IEnumerable<MarketEvent> activeEvents)
    {
        // 6. Déterminer quel événement s'applique à cette boisson
        // Priorité : 1. Spécificité (boissons ciblées) > 2. SessionId correspondant > 3. Temps
        var eventOnDrink = activeEvents
            .Where(e => e.Status != MarketEventStatus.Finished)
            .Where(e => e.DrinkIds.Count == 0 || e.DrinkIds.Contains(drink.Id))
            .OrderByDescending(e => e.DrinkIds.Count > 0) // Spécifiques d'abord
            .ThenByDescending(e => e.MarketSessionId == currentSessionId) // Session actuelle d'abord
            .ThenByDescending(e => e.StartAt) // Le plus récent d'abord
            .FirstOrDefault();

        decimal finalPrice = pureMarketPrice;

        if (eventOnDrink != null && eventOnDrink.Type == MarketEventType.FixedPrice)
        {
            finalPrice = eventOnDrink.Value;
        }
        else if (eventOnDrink != null && eventOnDrink.Type == MarketEventType.Discount)
        {
            finalPrice = pureMarketPrice * (1 - (eventOnDrink.Value / 100m));
            if (finalPrice < drink.MinPrice) finalPrice = drink.MinPrice;
        }

        return finalPrice;
    }
}
