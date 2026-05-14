using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;
using Babian.Domain.Interfaces;
using Babian.Domain.Exceptions;
using MediatR;

namespace Babian.BusinessLayers.MarketEvents.Features.ActivateEvent;

public class ActivateMarketEventCommandHandler : IRequestHandler<ActivateMarketEventCommand, MarketEvent>
{
    private readonly IMarketEventRepository _repository;
    private readonly IMarketSessionRepository _sessionRepository;
    private readonly IMarketConfigRepository _configRepository;
    private readonly IMarketNotificationService _notificationService;

    public ActivateMarketEventCommandHandler(
        IMarketEventRepository repository, 
        IMarketSessionRepository sessionRepository,
        IMarketConfigRepository configRepository,
        IMarketNotificationService notificationService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _configRepository = configRepository ?? throw new ArgumentNullException(nameof(configRepository));
        _notificationService = notificationService;
    }

    public async Task<MarketEvent> Handle(ActivateMarketEventCommand request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.EventId, cancellationToken);
        
        if (template == null)
            throw new NotFoundException("Événement introuvable.");

        if (template.BarmanId != request.BarmanId)
            throw new ForbiddenException("Vous n'êtes pas propriétaire de cet événement.");

        if (!template.IsTemplate)
            throw new ValidationException("Cet événement n'est pas un modèle (template) et ne peut être activé ainsi.");

        var activeSession = await _sessionRepository.GetActiveSessionAsync(request.BarmanId, cancellationToken);
        if (activeSession == null)
            throw new Exception("Aucune bourse ouverte. Impossible de lancer un événement sans bourse active.");

        var config = await _configRepository.GetByBarmanIdAsync(request.BarmanId, cancellationToken);
        if (config == null)
            throw new Exception("Configuration du marché introuvable.");

        // Calcul du prochain passage de cycle pour synchroniser l'activation
        var now = DateTime.UtcNow;
        var nextCycleAt = activeSession.LastPriceUpdateAt.AddSeconds(config.CycleDurationSeconds);
        
        // Si le temps actuel est déjà après (rattrapage nécessaire), on prend maintenant, 
        // sinon on cale sur le début du prochain cycle.
        var startAt = nextCycleAt > now ? nextCycleAt : now;

        var activeEvent = new MarketEvent
        {
            Id = Guid.NewGuid(),
            BarmanId = template.BarmanId,
            Name = template.Name,
            Description = template.Description,
            Type = template.Type,
            Value = template.Value,
            StartAt = startAt,
            EndAt = startAt.AddMinutes(request.DurationMinutes),
            DrinkIds = new System.Collections.Generic.List<Guid>(template.DrinkIds),
            IsTemplate = false,
            Status = MarketEventStatus.Active,
            MarketSessionId = activeSession.Id
        };

        await _repository.AddAsync(activeEvent, cancellationToken);

        await _notificationService.NotifyEventAsync(activeEvent.BarmanId, $"L'événement '{activeEvent.Name}' a été activé !", new { activeEvent.Id, activeEvent.Status });

        return activeEvent;
    }
}
