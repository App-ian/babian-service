using MediatR;
using System;

namespace Babian.BusinessLayers.Drinks.Features.ActivateDrink;

public record ActivateDrinkFromGlobalCommand(
    Guid BarmanId,
    Guid GlobalDrinkId,
    decimal MinPrice,
    decimal MaxPrice
) : IRequest<Guid>;
