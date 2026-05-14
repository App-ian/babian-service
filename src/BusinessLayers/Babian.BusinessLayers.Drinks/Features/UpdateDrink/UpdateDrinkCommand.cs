using MediatR;
using System;
using System.Text.Json.Serialization;

namespace Babian.BusinessLayers.Drinks.Features.UpdateDrink;

public record UpdateDrinkCommand(
    [property: JsonPropertyName("id")] Guid DrinkId,
    [property: JsonPropertyName("barmanId")] Guid BarmanId,
    [property: JsonPropertyName("basePrice")] decimal BasePrice,
    [property: JsonPropertyName("minPrice")] decimal MinPrice,
    [property: JsonPropertyName("maxPrice")] decimal MaxPrice,
    [property: JsonPropertyName("imageUrl")] string? ImageUrl,
    [property: JsonPropertyName("rowVersion")] Guid RowVersion
) : IRequest<bool>;
