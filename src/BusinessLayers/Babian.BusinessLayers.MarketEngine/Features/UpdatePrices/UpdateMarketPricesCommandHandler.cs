using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using Babian.Domain.Entities;
using Babian.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

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
    private readonly IMarketNotificationService _notificationService;
    private readonly ILogger<UpdateMarketPricesCommandHandler> _logger;
    
    // Protection contre le spam / Concurrence d'exécution pour un même barman
    private static readonly ConcurrentDictionary<Guid, byte> _activeUpdates = new();

    public UpdateMarketPricesCommandHandler(
        IMarketSessionRepository sessionRepository,
        IMarketConfigRepository configRepository,
        IDrinkRepository drinkRepository,
        IOrderRepository orderRepository,
        IPriceHistoryRepository historyRepository,
        IMarketEventRepository eventRepository,
        IPriceRankingService rankingService,
        IPriceCalculationService calculationService,
        IMarketEventApplier eventApplier,
        IMarketNotificationService notificationService,
        ILogger<UpdateMarketPricesCommandHandler> logger)
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
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateMarketPricesCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetActiveSessionAsync(request.OwnerId, cancellationToken);
        if (session == null) return false;

        // Début de la protection contre le Spam / Exécutions concurrentes
        if (!_activeUpdates.TryAdd(request.OwnerId, 0))
        {
            _logger.LogWarning("Une mise à jour des prix est déjà en cours pour le barman {OwnerId}. Demande ignorée.", request.OwnerId);
            throw new ConflictException("Une mise à jour du marché est déjà en cours. Veuillez patienter.");
        }

        try 
        {
            _logger.LogInformation("Début du cycle {CycleNumber} pour la session {SessionId} (Barman: {OwnerId})", 
                session.CurrentCycleNumber, session.Id, request.OwnerId);

        var config = await _configRepository.GetByBarmanIdAsync(request.OwnerId, cancellationToken);
        if (config == null) return false;

        var drinks = await _drinkRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);
        var activeEvents = await _eventRepository.GetActiveEventsByBarmanIdAsync(request.OwnerId, cancellationToken);

        bool hasTicked = false;

        while (request.IsManualForce || (!request.IncrementCycle) || (session.LastPriceUpdateAt.AddSeconds(config.CycleDurationSeconds) <= DateTime.UtcNow))
        {
            if (request.IsManualForce || request.IncrementCycle)
            {
                await ProcessEventStateTransitionsAsync(request.OwnerId, cancellationToken);
                // Reload active events to apply them to prices
                activeEvents = await _eventRepository.GetActiveEventsByBarmanIdAsync(request.OwnerId, cancellationToken);
            }

            var cycleCounts = await _orderRepository.GetCountsBySessionAsync(session.Id, session.CurrentCycleNumber, null, cancellationToken);
            
            var rankedDrinks = _rankingService.RankDrinks(drinks, cycleCounts);

            foreach (var item in rankedDrinks)
            {
                var drink = item.Drink;
                try
                {
                    decimal pureMarketPrice = _calculationService.CalculatePureMarketPrice(drink, item.Rank, item.Sales, config.RankingGroups);
                    
                    decimal finalPrice = _eventApplier.ApplyEvents(drink, pureMarketPrice, session.Id, activeEvents);

                    if (finalPrice != drink.CurrentPrice || pureMarketPrice != drink.MarketPrice)
                    {
                        decimal oldPrice = drink.CurrentPrice;
                        decimal variation = finalPrice - oldPrice;

                        drink.MarketPrice = pureMarketPrice;
                        drink.CurrentPrice = finalPrice;
                        
                        // Concurrency: we don't change RowVersion to a request value because the worker 
                        // is always "latest" by its own logic, but we must rotate it.
                        drink.RowVersion = Guid.NewGuid();

                        try
                        {
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
                        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
                        {
                            // Concurrency conflict: Admin likely modified the drink while worker was calculating.
                            // We skip this drink for this tick to avoid crashing the whole cycle.
                            _logger.LogWarning("Concurrence détectée pour {DrinkName}. Mise à jour ignorée pour ce cycle.", drink.Name);
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors du calcul des prix pour {DrinkName} : {Message}", drink.Name, ex.Message);
                    // On continue avec la boisson suivante au lieu de tout bloquer
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
            
            // Notification SignalR
            var updatedPrices = drinks.Select(d => new { d.Id, d.CurrentPrice, d.MarketPrice }).ToList();
            await _notificationService.NotifyPricesUpdatedAsync(request.OwnerId, updatedPrices);
        }

        return true;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Erreur de concurrence globale lors de la mise à jour du marché pour {OwnerId}.", request.OwnerId);
            throw new ConflictException("Le marché a été mis à jour par une autre action. Veuillez réessayer.");
        }
        finally
        {
            _activeUpdates.TryRemove(request.OwnerId, out _);
        }
    }

    private async Task ProcessEventStateTransitionsAsync(Guid barmanId, CancellationToken cancellationToken)
    {
        var allEvents = await _eventRepository.GetByBarmanIdAsync(barmanId, cancellationToken);
        var now = DateTime.UtcNow;
        int count = 0;

        foreach (var ev in allEvents)
        {
            if (ev.IsTemplate) continue;

            // Activation: Scheduled -> Live
            if (ev.Status == MarketEventStatus.Scheduled && ev.StartAt <= now)
            {
                _logger.LogInformation("Transition d'événement : {EventName} -> {NewStatus}", ev.Name, MarketEventStatus.Live);
                ev.Status = MarketEventStatus.Live;
                await _eventRepository.UpdateAsync(ev, cancellationToken);
                count++;
            }
            // Deactivation: Live -> Finished
            else if (ev.Status == MarketEventStatus.Live && ev.EndAt <= now)
            {
                _logger.LogInformation("Transition d'événement : {EventName} -> {NewStatus}", ev.Name, MarketEventStatus.Finished);
                ev.Status = MarketEventStatus.Finished;
                await _eventRepository.UpdateAsync(ev, cancellationToken);
                count++;
            }
        }

        if (count > 0)
        {
            _logger.LogInformation("Cycle forcé ou naturel : Traitement de {count} événements en attente (Activation/Clôture).", count);
            
            var activeEvents = allEvents.Where(e => e.Status == MarketEventStatus.Live).ToList();
            await _notificationService.NotifyEventAsync(barmanId, "Mise à jour des événements.", new { Type = "ActiveEventsUpdated", Events = activeEvents });
        }
    }
}

