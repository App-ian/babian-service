using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using Babian.Domain.Exceptions;
using MediatR;

namespace Babian.BusinessLayers.Drinks.Features.DeactivateDrink;

public class DeactivateDrinkCommandHandler : IRequestHandler<DeactivateDrinkCommand, bool>
{
    private readonly IDrinkRepository _drinkRepository;

    public DeactivateDrinkCommandHandler(IDrinkRepository drinkRepository)
    {
        _drinkRepository = drinkRepository ?? throw new ArgumentNullException(nameof(drinkRepository));
    }

    public async Task<bool> Handle(DeactivateDrinkCommand request, CancellationToken cancellationToken)
    {
        var drink = await _drinkRepository.GetByIdAsync(request.DrinkId, cancellationToken);

        if (drink == null)
            throw new NotFoundException($"Boisson {request.DrinkId} introuvable.");

        if (drink.OwnerId != request.BarmanId)
            throw new ForbiddenException("Vous n'êtes pas propriétaire de cette boisson.");

        drink.IsActive = false;
        await _drinkRepository.UpdateAsync(drink, cancellationToken);
        return true;
    }
}
