using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
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
        var drinks = await _drinkRepository.GetByOwnerIdAsync(request.BarmanId, cancellationToken);
        var drink = drinks.Find(d => d.Id == request.DrinkId);

        if (drink == null)
        {
            return false;
        }

        // 1. Business Validations
        if (request.MinPrice <= 0 || request.BasePrice <= 0 || request.MaxPrice <= 0)
            throw new Exception("Tous les prix doivent être supérieurs à 0.");

        if (request.MaxPrice <= request.MinPrice)
            throw new Exception("Le prix maximum doit être supérieur au prix minimum.");

        if (request.BasePrice < request.MinPrice || request.BasePrice > request.MaxPrice)
            throw new Exception("Le prix de base doit être compris entre le prix minimum et le prix maximum.");

        if (request.MaxPrice > 200)
            throw new Exception("Le prix maximum ne peut pas dépasser 200€ (seuil de sécurité).");

        // 2. Update entity (Prices only)
        drink.BasePrice = request.BasePrice;
        drink.MinPrice = request.MinPrice;
        drink.MaxPrice = request.MaxPrice;

        await _drinkRepository.UpdateAsync(drink, cancellationToken);
        return true;
    }
}
