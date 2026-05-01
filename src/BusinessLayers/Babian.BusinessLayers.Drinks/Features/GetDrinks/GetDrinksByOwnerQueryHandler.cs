using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using Babian.Domain.Entities;
using MediatR;

namespace Babian.BusinessLayers.Drinks.Features.GetDrinks;

public class GetDrinksByOwnerQueryHandler : IRequestHandler<GetDrinksByOwnerQuery, List<Drink>>
{
    private readonly IDrinkRepository _drinkRepository;

    public GetDrinksByOwnerQueryHandler(IDrinkRepository drinkRepository)
    {
        _drinkRepository = drinkRepository;
    }

    public async Task<List<Drink>> Handle(GetDrinksByOwnerQuery request, CancellationToken cancellationToken)
    {
        return await _drinkRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);
    }
}
