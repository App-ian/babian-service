using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;
using Babian.Domain.Interfaces;
using Babian.Infrastructure.Persistence;
using MediatR;

namespace Babian.BusinessLayers.MarketEvents.Features.CreateEvent;

public class CreateMarketEventCommandHandler : IRequestHandler<CreateMarketEventCommand, MarketEvent>
{
    private readonly IMarketEventRepository _repository;
    private readonly IMarketSessionRepository _sessionRepository;

    public CreateMarketEventCommandHandler(IMarketEventRepository repository, IMarketSessionRepository sessionRepository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
    }

    public async Task<MarketEvent> Handle(CreateMarketEventCommand request, CancellationToken cancellationToken)
    {
        if (request.EndAt <= request.StartAt)
        {
            throw new ArgumentException("La date de fin doit être postérieure à la date de début.");
        }

        Guid? sessionId = null;
        if (!request.IsTemplate)
        {
             var activeSession = await _sessionRepository.GetActiveSessionAsync(request.BarmanId, cancellationToken);
             if (activeSession != null)
             {
                 sessionId = activeSession.Id;
             }
        }

        var marketEvent = new MarketEvent
        {
            Id = Guid.NewGuid(),
            BarmanId = request.BarmanId,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Value = request.Value,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            DrinkIds = request.DrinkIds,
            IsTemplate = request.IsTemplate,
            Status = request.Status,
            MarketSessionId = sessionId,
            Recurrence = request.Recurrence
        };

        await _repository.AddAsync(marketEvent, cancellationToken);

        return marketEvent;
    }
}
