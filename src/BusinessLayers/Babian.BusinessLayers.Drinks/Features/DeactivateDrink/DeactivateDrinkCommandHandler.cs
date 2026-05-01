using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
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
        var drinks = await _drinkRepository.GetByOwnerIdAsync(request.BarmanId, cancellationToken);
        var drink = drinks.Find(d => d.Id == request.DrinkId);

        if (drink == null)
        {
            return false;
        }

        drink.IsActive = false;
        await _drinkRepository.UpdateAsync(drink, cancellationToken);
        return true;
    }
}
