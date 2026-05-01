using System;
using System.Collections.Generic;
using MediatR;

namespace Babian.BusinessLayers.MarketEngine.Features.GetHistory;

public record GetLastPriceVariationsQuery(Guid SessionId) : IRequest<List<PriceVariationDto>>;

public class PriceVariationDto
{
    public Guid DrinkId { get; set; }
    public decimal Variation { get; set; }
}
