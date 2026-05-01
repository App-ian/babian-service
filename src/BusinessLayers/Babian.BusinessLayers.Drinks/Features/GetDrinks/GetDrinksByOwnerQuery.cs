using System;
using System.Collections.Generic;
using Babian.Domain.Entities;
using MediatR;

namespace Babian.BusinessLayers.Drinks.Features.GetDrinks;

public record GetDrinksByOwnerQuery(Guid OwnerId) : IRequest<List<Drink>>;
