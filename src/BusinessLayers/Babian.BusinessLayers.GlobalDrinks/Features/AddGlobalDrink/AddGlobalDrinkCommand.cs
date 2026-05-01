using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Entities;
using Babian.Domain.Interfaces;
using MediatR;

namespace Babian.BusinessLayers.GlobalDrinks.Features.AddGlobalDrink;

public record AddGlobalDrinkCommand(
    string Name,
    decimal DefaultBasePrice,
    string? ImageUrl,
    string? Volume,
    string? Category,
    string? Plu,
    bool IsActive = true
) : IRequest<Guid>;

public class AddGlobalDrinkCommandHandler : IRequestHandler<AddGlobalDrinkCommand, Guid>
{
    private readonly IGlobalDrinkRepository _repository;

    public AddGlobalDrinkCommandHandler(IGlobalDrinkRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(AddGlobalDrinkCommand request, CancellationToken cancellationToken)
    {
        var globalDrink = new GlobalDrink
        {
            Name = request.Name,
            DefaultBasePrice = request.DefaultBasePrice,
            ImageUrl = request.ImageUrl,
            Volume = request.Volume,
            Category = request.Category,
            Plu = request.Plu,
            IsActive = request.IsActive
        };

        await _repository.AddAsync(globalDrink, cancellationToken);
        return globalDrink.Id;
    }
}
