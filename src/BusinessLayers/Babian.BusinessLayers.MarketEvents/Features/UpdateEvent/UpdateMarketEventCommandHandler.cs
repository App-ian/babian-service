using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;
using Babian.Domain.Interfaces;
using MediatR;

namespace Babian.BusinessLayers.MarketEvents.Features.UpdateEvent;

public class UpdateMarketEventCommandHandler : IRequestHandler<UpdateMarketEventCommand, bool>
{
    private readonly IMarketEventRepository _repository;

    public UpdateMarketEventCommandHandler(IMarketEventRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<bool> Handle(UpdateMarketEventCommand request, CancellationToken cancellationToken)
    {
        var marketEvent = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (marketEvent == null || marketEvent.BarmanId != request.BarmanId)
        {
            return false;
        }

        if (request.EndAt <= request.StartAt)
        {
            throw new ArgumentException("La date de fin doit être postérieure à la date de début.");
        }

        marketEvent.Name = request.Name;
        marketEvent.Description = request.Description;
        marketEvent.Type = request.Type;
        marketEvent.Value = request.Value;
        marketEvent.StartAt = request.StartAt;
        marketEvent.EndAt = request.EndAt;
        marketEvent.DrinkIds = request.DrinkIds;
        marketEvent.IsTemplate = request.IsTemplate;
        marketEvent.Status = request.Status;
        marketEvent.Recurrence = request.Recurrence;

        await _repository.UpdateAsync(marketEvent, cancellationToken);

        return true;
    }
}
