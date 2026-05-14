using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using MediatR;

namespace Babian.BusinessLayers.Drinks.Features.GetDrinks;

public class GetDrinksByOwnerQueryHandler : IRequestHandler<GetDrinksByOwnerQuery, List<DrinkDto>>
{
    private readonly IDrinkRepository _drinkRepository;

    public GetDrinksByOwnerQueryHandler(IDrinkRepository drinkRepository)
    {
        _drinkRepository = drinkRepository;
    }

    public async Task<List<DrinkDto>> Handle(GetDrinksByOwnerQuery request, CancellationToken cancellationToken)
    {
        var drinks = await _drinkRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);
        
        return drinks.Select(d => new DrinkDto(
            d.Id,
            d.Name,
            d.BasePrice,
            d.CurrentPrice,
            d.MinPrice,
            d.MaxPrice,
            d.MarketPrice,
            d.ImageUrl,
            d.Volume,
            d.Category,
            d.Plu,
            d.IsActive,
            d.OwnerId,
            d.MarketSessionId,
            d.GlobalDrinkId,
            d.RowVersion,
            d.CreatedAt
        )).ToList();
    }
}
