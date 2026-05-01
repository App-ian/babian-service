using System;
using System.Collections.Generic;
using MediatR;

namespace Babian.BusinessLayers.MarketEngine.Features.GetPosPrices;

public record PosPriceItemDto(string Plu, int PriceCents);

public record PosMarketPricesDto(
    string RestaurantId,
    DateTime Timestamp,
    List<PosPriceItemDto> Prices
);

public record GetPosPricesQuery(Guid OwnerId, string RestaurantId) : IRequest<PosMarketPricesDto>;
