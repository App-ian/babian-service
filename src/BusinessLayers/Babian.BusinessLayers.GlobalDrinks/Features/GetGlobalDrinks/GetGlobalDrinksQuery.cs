using MediatR;
using System.Collections.Generic;

namespace Babian.BusinessLayers.GlobalDrinks.Features.GetGlobalDrinks;

public record GlobalDrinkDto(Guid Id, string Name, decimal DefaultBasePrice, string? Category, string? Volume, bool IsActive, string? ImageUrl, string? Plu);


public record GetGlobalDrinksQuery() : IRequest<List<GlobalDrinkDto>>;
