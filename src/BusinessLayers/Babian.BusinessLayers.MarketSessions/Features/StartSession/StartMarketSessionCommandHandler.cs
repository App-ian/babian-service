using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using Babian.Domain.Entities;
using MediatR;

namespace Babian.BusinessLayers.MarketSessions.Features.StartSession;

public class StartMarketSessionCommandHandler : IRequestHandler<StartMarketSessionCommand, Guid>
{
    private readonly IMarketSessionRepository _sessionRepository;
    private readonly IDrinkRepository _drinkRepository;
    private readonly IMarketEventRepository _eventRepository;
    private readonly IMarketConfigRepository _configRepository;
    private readonly IMarketNotificationService _notificationService;

    public StartMarketSessionCommandHandler(
        IMarketSessionRepository sessionRepository,
        IDrinkRepository drinkRepository,
        IMarketEventRepository eventRepository,
        IMarketConfigRepository configRepository,
        IMarketNotificationService notificationService)
    {
        _sessionRepository = sessionRepository;
        _drinkRepository = drinkRepository;
        _eventRepository = eventRepository;
        _configRepository = configRepository;
        _notificationService = notificationService;
    }

    public async Task<Guid> Handle(StartMarketSessionCommand request, CancellationToken cancellationToken)
    {
        // 1. Vérifier s'il y a déjà une session active
        var activeSession = await _sessionRepository.GetActiveSessionAsync(request.OwnerId, cancellationToken);
        if (activeSession != null)
        {
            return activeSession.Id; // On retourne la session existante si déjà ouverte
        }

        // 2. Créer la nouvelle session
        var newSession = new MarketSession
        {
            Id = Guid.NewGuid(),
            OwnerId = request.OwnerId,
            Name = request.Name,
            StartTime = DateTime.UtcNow,
            IsActive = true,
            CurrentCycleNumber = 1,
            LastPriceUpdateAt = DateTime.UtcNow
        };
        // 3. Réinitialiser le prix courant et le prix de marché de toutes les boissons actives au prix de base
        var drinks = await _drinkRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);
        foreach (var drink in drinks)
        {
            if (drink.CurrentPrice != drink.BasePrice || drink.MarketPrice != drink.BasePrice)
            {
                drink.CurrentPrice = drink.BasePrice;
                drink.MarketPrice = drink.BasePrice;
                await _drinkRepository.UpdateAsync(drink, cancellationToken);
            }
        }

        await _sessionRepository.CreateAsync(newSession, cancellationToken);

        // Notification SignalR pour mettre à jour les écrans publics (prévisualisation)
        await _notificationService.NotifyEventAsync(request.OwnerId, "La bourse a démarré !", new { Type = "MarketStarted", SessionId = newSession.Id });
        
        // 4. Adopter les événements planifiés orphelins (MarketSessionId == null)
        // uniquement s'ils commencent pendant la durée prévue de la bourse
        var events = await _eventRepository.GetByBarmanIdAsync(request.OwnerId, cancellationToken);
        
        // Calcul de la durée estimée de la bourse
        var config = await _configRepository.GetByBarmanIdAsync(request.OwnerId, cancellationToken);
        var durationSeconds = (config?.TotalCycles ?? 8 * 60 * 60 / (config?.CycleDurationSeconds ?? 60)) * (config?.CycleDurationSeconds ?? 60);
        // Si cycles illimités ou non définis, on prend 8h par défaut
        if (config?.TotalCycles == null) durationSeconds = 8 * 60 * 60; 

        var sessionEndTime = DateTime.UtcNow.AddSeconds(durationSeconds);
        
        foreach(var ev in events)
        {
            // Cas 1 : Événement planifié unique
            if (!ev.IsTemplate && ev.MarketSessionId == null && ev.ParentEventId == null && ev.Recurrence == RecurrenceType.None && ev.StartAt < sessionEndTime)
            {
                ev.MarketSessionId = newSession.Id;
                await _eventRepository.UpdateAsync(ev, cancellationToken);
            }
            // Cas 2 : Événement récurrent (Générateur)
            else if (ev.Recurrence != RecurrenceType.None && ev.ParentEventId == null)
            {
                // Si l'occurrence prochaine tombe dans la fenêtre de la bourse
                if (ev.StartAt < sessionEndTime)
                {
                    // On crée une instance pour cette session
                    var occurrence = new MarketEvent
                    {
                        Id = Guid.NewGuid(),
                        BarmanId = ev.BarmanId,
                        Name = ev.Name,
                        Description = ev.Description,
                        Type = ev.Type,
                        Value = ev.Value,
                        StartAt = ev.StartAt,
                        EndAt = ev.EndAt,
                        DrinkIds = new List<Guid>(ev.DrinkIds),
                        IsTemplate = false,
                        Status = MarketEventStatus.Scheduled,
                        MarketSessionId = newSession.Id,
                        ParentEventId = ev.Id,
                        Recurrence = RecurrenceType.None
                    };
                    await _eventRepository.AddAsync(occurrence, cancellationToken);

                    // On prépare le générateur pour la fois suivante
                    ev.StartAt = CalculateNextOccurrence(ev.StartAt, ev.Recurrence);
                    ev.EndAt = CalculateNextOccurrence(ev.EndAt, ev.Recurrence);
                    await _eventRepository.UpdateAsync(ev, cancellationToken);
                }
            }
        }

        return newSession.Id;
    }

    private DateTime CalculateNextOccurrence(DateTime current, RecurrenceType type)
    {
        return type switch
        {
            RecurrenceType.Daily => current.AddDays(1),
            RecurrenceType.Weekly => current.AddDays(7),
            RecurrenceType.Monthly => current.AddMonths(1),
            RecurrenceType.Yearly => current.AddYears(1),
            _ => current
        };
    }
}
