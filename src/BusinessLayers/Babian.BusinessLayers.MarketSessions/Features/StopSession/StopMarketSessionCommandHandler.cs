using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using Babian.Domain.Entities;
using MediatR;

namespace Babian.BusinessLayers.MarketSessions.Features.StopSession;

public class StopMarketSessionCommandHandler : IRequestHandler<StopMarketSessionCommand, bool>
{
    private readonly IMarketSessionRepository _sessionRepository;
    private readonly IMarketEventRepository _eventRepository;
    private readonly IMarketNotificationService _notificationService;

    public StopMarketSessionCommandHandler(
        IMarketSessionRepository sessionRepository, 
        IMarketEventRepository eventRepository,
        IMarketNotificationService notificationService)
    {
        _sessionRepository = sessionRepository;
        _eventRepository = eventRepository;
        _notificationService = notificationService;
    }

    public async Task<bool> Handle(StopMarketSessionCommand request, CancellationToken cancellationToken)
    {
        var activeSession = await _sessionRepository.GetActiveSessionAsync(request.OwnerId, cancellationToken);
        if (activeSession == null) return false;

        activeSession.IsActive = false;
        activeSession.EndTime = DateTime.UtcNow;

        await _sessionRepository.UpdateAsync(activeSession, cancellationToken);

        // 4. Gérer les événements rattachés à cette session
        var events = await _eventRepository.GetByBarmanIdAsync(activeSession.OwnerId, cancellationToken);
        var now = DateTime.UtcNow;
        foreach (var ev in events)
        {
            if (ev.MarketSessionId == activeSession.Id)
            {
                if (ev.StartAt > now)
                {
                    // L'événement est encore dans le futur : on le libère (remise sur le bureau)
                    ev.MarketSessionId = null;
                }
                else
                {
                    // L'événement a déjà commencé ou est fini : on le clôture définitivement
                    ev.Status = MarketEventStatus.Finished;
                }
                await _eventRepository.UpdateAsync(ev, cancellationToken);
            }
        }

        // Notification SignalR
        await _notificationService.NotifyEventAsync(activeSession.OwnerId, "La bourse est fermée.", new { Type = "MarketStopped" });

        return true;
    }
}
