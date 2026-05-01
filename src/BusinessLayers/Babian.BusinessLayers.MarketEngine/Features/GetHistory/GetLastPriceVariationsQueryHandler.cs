using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using MediatR;

namespace Babian.BusinessLayers.MarketEngine.Features.GetHistory;

public class GetLastPriceVariationsQueryHandler : IRequestHandler<GetLastPriceVariationsQuery, List<PriceVariationDto>>
{
    private readonly IPriceHistoryRepository _historyRepository;

    public GetLastPriceVariationsQueryHandler(IPriceHistoryRepository historyRepository)
    {
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
    }

    public async Task<List<PriceVariationDto>> Handle(GetLastPriceVariationsQuery request, CancellationToken cancellationToken)
    {
        var variations = await _historyRepository.GetLastVariationsBySessionAsync(request.SessionId, cancellationToken);
        
        return variations.Select(kvp => new PriceVariationDto
        {
            DrinkId = kvp.Key,
            Variation = kvp.Value
        }).ToList();
    }
}
