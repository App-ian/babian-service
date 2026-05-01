using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using MediatR;

namespace Babian.BusinessLayers.GlobalDrinks.Features.UpdateGlobalDrink;

public record UpdateGlobalDrinkCommand(
    Guid Id,
    string Name,
    decimal DefaultBasePrice,
    string? ImageUrl,
    string? Volume,
    string? Category,
    string? Plu,
    bool IsActive
) : IRequest;

public class UpdateGlobalDrinkCommandHandler : IRequestHandler<UpdateGlobalDrinkCommand>
{
    private readonly IGlobalDrinkRepository _repository;

    public UpdateGlobalDrinkCommandHandler(IGlobalDrinkRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(UpdateGlobalDrinkCommand request, CancellationToken cancellationToken)
    {
        var globalDrink = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (globalDrink == null)
        {
            throw new Exception($"GlobalDrink with ID {request.Id} not found.");
        }

        globalDrink.Name = request.Name;
        globalDrink.DefaultBasePrice = request.DefaultBasePrice;
        globalDrink.ImageUrl = request.ImageUrl;
        globalDrink.Volume = request.Volume;
        globalDrink.Category = request.Category;
        globalDrink.Plu = request.Plu;
        globalDrink.IsActive = request.IsActive;

        await _repository.UpdateAsync(globalDrink, cancellationToken);
    }
}
