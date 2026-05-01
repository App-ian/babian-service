using MediatR;
using System;

namespace Babian.BusinessLayers.Drinks.Features.UpdatePrice;

public record UpdateDrinkPriceCommand(
    Guid DrinkId,
    Guid BarmanId,
    decimal Price
) : IRequest<bool>;
