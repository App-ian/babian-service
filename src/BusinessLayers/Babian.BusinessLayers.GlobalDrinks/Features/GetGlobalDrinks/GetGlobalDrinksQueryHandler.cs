using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using MediatR;

namespace Babian.BusinessLayers.GlobalDrinks.Features.GetGlobalDrinks;

public class GetGlobalDrinksQueryHandler : IRequestHandler<GetGlobalDrinksQuery, List<GlobalDrinkDto>>
{
    private readonly IGlobalDrinkRepository _repository;

    public GetGlobalDrinksQueryHandler(IGlobalDrinkRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<List<GlobalDrinkDto>> Handle(GetGlobalDrinksQuery request, CancellationToken cancellationToken)
    {
        var globalDrinks = await _repository.GetAllAsync(cancellationToken);
        
        return globalDrinks.Select(d => new GlobalDrinkDto(
            d.Id,
            d.Name,
            d.DefaultBasePrice,
            d.Category,
            d.Volume,
            d.IsActive,
            d.ImageUrl,
            d.Plu
        )).ToList();
    }
}
