using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using Babian.Domain.Exceptions;
using MediatR;

namespace Babian.BusinessLayers.Drinks.Features.UpdateDrink;

public class UpdateDrinkCommandHandler : IRequestHandler<UpdateDrinkCommand, bool>
{
    private readonly IDrinkRepository _drinkRepository;

    public UpdateDrinkCommandHandler(IDrinkRepository drinkRepository)
    {
        _drinkRepository = drinkRepository ?? throw new ArgumentNullException(nameof(drinkRepository));
    }

    public async Task<bool> Handle(UpdateDrinkCommand request, CancellationToken cancellationToken)
    {
        var drink = await _drinkRepository.GetByIdAsync(request.DrinkId, cancellationToken);

        if (drink == null)
            throw new NotFoundException($"Boisson {request.DrinkId} introuvable.");

        if (drink.OwnerId != request.BarmanId)
            throw new ForbiddenException("Vous n'êtes pas propriétaire de cette boisson.");

        // 2. Update entity (Prices only)
        drink.BasePrice = request.BasePrice;
        drink.MinPrice = request.MinPrice;
        drink.MaxPrice = request.MaxPrice;

        await _drinkRepository.UpdateAsync(drink, cancellationToken);
        return true;
    }
}
