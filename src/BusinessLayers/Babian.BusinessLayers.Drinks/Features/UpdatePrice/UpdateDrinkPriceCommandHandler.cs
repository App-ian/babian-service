using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using MediatR;

namespace Babian.BusinessLayers.Drinks.Features.UpdatePrice;

public class UpdateDrinkPriceCommandHandler : IRequestHandler<UpdateDrinkPriceCommand, bool>
{
    private readonly IDrinkRepository _drinkRepository;

    public UpdateDrinkPriceCommandHandler(IDrinkRepository drinkRepository)
    {
        _drinkRepository = drinkRepository ?? throw new ArgumentNullException(nameof(drinkRepository));
    }

    public async Task<bool> Handle(UpdateDrinkPriceCommand request, CancellationToken cancellationToken)
    {
        var drink = await _drinkRepository.GetByIdAsync(request.DrinkId, cancellationToken);

        if (drink == null || drink.OwnerId != request.BarmanId)
        {
            return false;
        }

        drink.MarketPrice = request.Price;
        await _drinkRepository.UpdateAsync(drink, cancellationToken);
        
        return true;
    }
}
