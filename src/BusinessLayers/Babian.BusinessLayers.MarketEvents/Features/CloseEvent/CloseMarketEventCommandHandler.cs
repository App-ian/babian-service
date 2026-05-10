using Babian.Domain.Entities;
using Babian.Domain.Interfaces;
using Babian.Domain.Exceptions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Babian.BusinessLayers.MarketEvents.Features.CloseEvent;

public class CloseMarketEventCommandHandler : IRequestHandler<CloseMarketEventCommand, bool>
{
    private readonly IMarketEventRepository _repository;

    public CloseMarketEventCommandHandler(IMarketEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(CloseMarketEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await _repository.GetByIdAsync(request.EventId, cancellationToken);

        if (@event == null)
            throw new NotFoundException($"Événement {request.EventId} introuvable.");

        if (@event.BarmanId != request.BarmanId)
            throw new ForbiddenException("Vous n'êtes pas propriétaire de cet événement.");

        // Only allow closing active or scheduled events
        if (@event.Status == MarketEventStatus.Finished)
        {
            return true; // Already finished
        }

        @event.Status = MarketEventStatus.Finished;
        @event.EndAt = DateTime.UtcNow;

        await _repository.UpdateAsync(@event, cancellationToken);
        return true;
    }
}
