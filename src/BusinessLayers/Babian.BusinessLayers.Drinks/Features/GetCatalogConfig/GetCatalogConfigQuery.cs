using System;
using System.Collections.Generic;
using MediatR;

namespace Babian.BusinessLayers.Drinks.Features.GetCatalogConfig;

public record PosCatalogItemDto(string Plu, string Name, bool IsActive);

public record PosCatalogRangeDto(string Start, string End);

public record PosCatalogConfigDto(
    string RestaurantId,
    PosCatalogRangeDto ActiveRange,
    List<PosCatalogItemDto> Items
);

public record GetCatalogConfigQuery(Guid OwnerId, string RestaurantId) : IRequest<PosCatalogConfigDto>;
