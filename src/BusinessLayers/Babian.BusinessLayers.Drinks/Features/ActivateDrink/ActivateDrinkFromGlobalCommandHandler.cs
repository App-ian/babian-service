using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using Babian.Domain.Entities;
using MediatR;

namespace Babian.BusinessLayers.Drinks.Features.ActivateDrink;

public class ActivateDrinkFromGlobalCommandHandler : IRequestHandler<ActivateDrinkFromGlobalCommand, Guid>
{
    private readonly IDrinkRepository _repository;

    public ActivateDrinkFromGlobalCommandHandler(IDrinkRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<Guid> Handle(ActivateDrinkFromGlobalCommand request, CancellationToken cancellationToken)
    {
        // 1. Check if global drink exists
        var globalDrink = await _repository.GetGlobalDrinkByIdAsync(request.GlobalDrinkId, cancellationToken);
        if (globalDrink == null)
        {
            throw new Exception("Boisson globale introuvable.");
        }

        // 2. Check if already exists (active or not)
        var existing = await _repository.GetByOwnerAndGlobalIdAsync(request.BarmanId, request.GlobalDrinkId, cancellationToken);
        if (existing != null)
        {
            if (!existing.IsActive)
            {
                existing.IsActive = true;
                existing.MinPrice = request.MinPrice;
                existing.MaxPrice = request.MaxPrice;
                await _repository.UpdateAsync(existing, cancellationToken);
            }
            return existing.Id;
        }

        // 3. Create new personal drink
        var drink = new Drink
        {
            Id = Guid.NewGuid(),
            Name = globalDrink.Name,
            BasePrice = globalDrink.DefaultBasePrice,
            CurrentPrice = globalDrink.DefaultBasePrice,
            MarketPrice = globalDrink.DefaultBasePrice,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            ImageUrl = globalDrink.ImageUrl,
            Volume = globalDrink.Volume,
            Category = globalDrink.Category,
            Plu = globalDrink.Plu,
            OwnerId = request.BarmanId,
            GlobalDrinkId = globalDrink.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _repository.AddAsync(drink, cancellationToken);
        return drink.Id;
    }
}
