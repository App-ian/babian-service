using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using Babian.Domain.Interfaces;
using Babian.Domain.Interfaces;
using Babian.Domain.Entities;
using MediatR;

namespace Babian.BusinessLayers.MarketEngine.Features.UpdatePrices;

public class UpdateMarketPricesCommandHandler : IRequestHandler<UpdateMarketPricesCommand, bool>
{
    private readonly IMarketSessionRepository _sessionRepository;
    private readonly IMarketConfigRepository _configRepository;
    private readonly IDrinkRepository _drinkRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPriceHistoryRepository _historyRepository;
    private readonly IMarketEventRepository _eventRepository;
    private readonly IPriceCalculator _priceCalculator;

    public UpdateMarketPricesCommandHandler(
        IMarketSessionRepository sessionRepository,
        IMarketConfigRepository configRepository,
        IDrinkRepository drinkRepository,
        IOrderRepository orderRepository,
        IPriceHistoryRepository historyRepository,
        IMarketEventRepository eventRepository,
        IPriceCalculator priceCalculator)
    {
        _sessionRepository = sessionRepository;
        _configRepository = configRepository;
        _drinkRepository = drinkRepository;
        _orderRepository = orderRepository;
        _historyRepository = historyRepository;
        _eventRepository = eventRepository;
        _priceCalculator = priceCalculator;
    }

    public async Task<bool> Handle(UpdateMarketPricesCommand request, CancellationToken cancellationToken)
    {
        // 1. Trouver la session active
        var session = await _sessionRepository.GetActiveSessionAsync(request.OwnerId, cancellationToken);
        if (session == null) return false;

        // 2. Récupérer la config du barman
        var config = await _configRepository.GetByBarmanIdAsync(request.OwnerId, cancellationToken);
        if (config == null) return false;

        // 3. Récupérer toutes les boissons actives du barman
        var drinks = await _drinkRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);
        
        // 5. Récupérer les événements actifs
        var activeEvents = await _eventRepository.GetActiveEventsByBarmanIdAsync(request.OwnerId, cancellationToken);

        bool hasTicked = false;

        // Boucle déterministe de rattrapage temporel (Fixed-Time Step)
        while (request.IsManualForce || (!request.IncrementCycle) || (session.LastPriceUpdateAt.AddSeconds(config.CycleDurationSeconds) <= DateTime.UtcNow))
        {
            // 6. Récupérer les commandes du cycle actuel pour le calcul de la demande
            var cycleCounts = await _orderRepository.GetCountsBySessionAsync(session.Id, session.CurrentCycleNumber, null, cancellationToken);

        // 6. Classer les boissons par ventes du cycle (du plus vendu au moins vendu)
        var rankedDrinks = drinks
            .Select(d => new
            {
                Drink = d,
                Sales = cycleCounts.TryGetValue(d.Id, out var sales) ? sales : 0
            })
            .OrderByDescending(x => x.Sales)
            .ThenBy(x => Guid.NewGuid())
            .ToList();

        var rankingGroups = config.RankingGroups?
            .OrderBy(g => g.MaxRank)
            .ToList();

        for (int rank = 1; rank <= rankedDrinks.Count; rank++)
        {
            var item = rankedDrinks[rank - 1];
            var drink = item.Drink;

            // Trouver le groupe de classement correspondant au rang actuel
            decimal coefficient;
            if (rankingGroups != null && rankingGroups.Count > 0)
            {
                var group = rankingGroups.FirstOrDefault(g => rank <= g.MaxRank)
                            ?? rankingGroups.Last(); // Fallback: dernier groupe
                coefficient = group.Coefficient;
            }
            else
            {
                // Aucun groupe configuré : fallback binaire
                coefficient = item.Sales > 0 ? 0.15m : -0.15m;
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
                targetPrice = drink.MarketPrice;
                speed = 0;
            }

            // 6. Déterminer quel événement s'applique à cette boisson
            // Priorité : 1. Spécificité (boissons ciblées) > 2. SessionId correspondant > 3. Temps
            var eventOnDrink = activeEvents
                .Where(e => e.Status != MarketEventStatus.Finished)
                .Where(e => e.DrinkIds.Count == 0 || e.DrinkIds.Contains(drink.Id))
                .OrderByDescending(e => e.DrinkIds.Count > 0) // Spécifiques d'abord
                .ThenByDescending(e => e.MarketSessionId == session.Id) // Session actuelle d'abord
                .ThenByDescending(e => e.StartAt) // Le plus récent d'abord
                .FirstOrDefault();

            // Calcul asymptotique (pour le prix de marché pur)
            decimal pureMarketPrice = speed == 0
                ? drink.MarketPrice
                : _priceCalculator.CalculateNextPrice(drink.MarketPrice, drink.MinPrice, drink.MaxPrice, targetPrice, speed);

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

            // 7. Mise à jour si le prix a changé
            if (finalPrice != drink.CurrentPrice || pureMarketPrice != drink.MarketPrice)
            {
                decimal oldPrice = drink.CurrentPrice;
                decimal variation = finalPrice - oldPrice;

                drink.MarketPrice = pureMarketPrice;
                drink.CurrentPrice = finalPrice;
                await _drinkRepository.UpdateAsync(drink, cancellationToken);

                await _historyRepository.AddAsync(new PriceHistory
                {
                    DrinkId = drink.Id,
                    Price = finalPrice,
                    Variation = variation,
                    CycleNumber = session.CurrentCycleNumber,
                    MarketSessionId = session.Id,
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);
            }
        }


            // 8. Incrémenter le cycle et mettre à jour le timestamp SI demandé
            if (request.IncrementCycle)
            {
                session.CurrentCycleNumber++;
                
                if (config.TotalCycles.HasValue && session.CurrentCycleNumber > config.TotalCycles.Value)
                {
                    session.IsActive = false;
                    session.EndTime = DateTime.UtcNow;
                }
                else
                {
                    if (request.IsManualForce)
                    {
                        session.LastPriceUpdateAt = DateTime.UtcNow;
                    }
                    else
                    {
                        session.LastPriceUpdateAt = session.LastPriceUpdateAt.AddSeconds(config.CycleDurationSeconds);
                    }
                }
                hasTicked = true;
            }

            // Un seul passage si on force manuellement le cycle ou si on met juste à jour les prix (sans incrémenter)
            if (request.IsManualForce || !request.IncrementCycle || !session.IsActive) break; 
        }

        if (hasTicked || !request.IncrementCycle)
        {
            await _sessionRepository.UpdateAsync(session, cancellationToken);
        }

        return true;
    }
}
