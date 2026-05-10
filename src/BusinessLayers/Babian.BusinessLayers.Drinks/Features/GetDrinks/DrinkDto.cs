using System;

namespace Babian.BusinessLayers.Drinks.Features.GetDrinks;

public record DrinkDto(
    Guid Id,
    string Name,
    decimal BasePrice,
    decimal CurrentPrice,
    decimal MinPrice,
    decimal MaxPrice,
    decimal MarketPrice,
    string? ImageUrl,
    string? Volume,
    string? Category,
    string? Plu,
    bool IsActive,
    Guid OwnerId,
    Guid? MarketSessionId,
    Guid GlobalDrinkId,
    DateTime CreatedAt
);
