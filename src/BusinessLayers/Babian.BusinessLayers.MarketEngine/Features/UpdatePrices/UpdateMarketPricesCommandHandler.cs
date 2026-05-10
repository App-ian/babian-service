using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    
    private readonly IPriceRankingService _rankingService;
    private readonly IPriceCalculationService _calculationService;
    private readonly IMarketEventApplier _eventApplier;

    public UpdateMarketPricesCommandHandler(
        IMarketSessionRepository sessionRepository,
        IMarketConfigRepository configRepository,
        IDrinkRepository drinkRepository,
        IOrderRepository orderRepository,
        IPriceHistoryRepository historyRepository,
        IMarketEventRepository eventRepository,
        IPriceRankingService rankingService,
        IPriceCalculationService calculationService,
        IMarketEventApplier eventApplier)
    {
        _sessionRepository = sessionRepository;
        _configRepository = configRepository;
        _drinkRepository = drinkRepository;
        _orderRepository = orderRepository;
        _historyRepository = historyRepository;
        _eventRepository = eventRepository;
        
        _rankingService = rankingService;
        _calculationService = calculationService;
        _eventApplier = eventApplier;
    }

    public async Task<bool> Handle(UpdateMarketPricesCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetActiveSessionAsync(request.OwnerId, cancellationToken);
        if (session == null) return false;

        var config = await _configRepository.GetByBarmanIdAsync(request.OwnerId, cancellationToken);
        if (config == null) return false;

        var drinks = await _drinkRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);
        var activeEvents = await _eventRepository.GetActiveEventsByBarmanIdAsync(request.OwnerId, cancellationToken);

        bool hasTicked = false;

        while (request.IsManualForce || (!request.IncrementCycle) || (session.LastPriceUpdateAt.AddSeconds(config.CycleDurationSeconds) <= DateTime.UtcNow))
        {
            var cycleCounts = await _orderRepository.GetCountsBySessionAsync(session.Id, session.CurrentCycleNumber, null, cancellationToken);
            
            var rankedDrinks = _rankingService.RankDrinks(drinks, cycleCounts);

            foreach (var item in rankedDrinks)
            {
                var drink = item.Drink;
                
                decimal pureMarketPrice = _calculationService.CalculatePureMarketPrice(drink, item.Rank, item.Sales, config.RankingGroups);
                
                decimal finalPrice = _eventApplier.ApplyEvents(drink, pureMarketPrice, session.Id, activeEvents);

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

            if (request.IsManualForce || !request.IncrementCycle || !session.IsActive) break; 
        }

        if (hasTicked || !request.IncrementCycle)
        {
            await _sessionRepository.UpdateAsync(session, cancellationToken);
        }

        return true;
    }
}

