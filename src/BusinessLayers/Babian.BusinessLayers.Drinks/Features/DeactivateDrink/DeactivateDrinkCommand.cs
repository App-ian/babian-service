using MediatR;
using System;

namespace Babian.BusinessLayers.Drinks.Features.DeactivateDrink;

public record DeactivateDrinkCommand(
    Guid DrinkId,
    Guid BarmanId
) : IRequest<bool>;
