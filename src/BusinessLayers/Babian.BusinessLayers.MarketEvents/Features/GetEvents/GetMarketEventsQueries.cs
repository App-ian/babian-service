using Babian.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;

namespace Babian.BusinessLayers.MarketEvents.Features.GetEvents;

public record GetMarketEventsQuery(Guid BarmanId) : IRequest<List<MarketEvent>>;
public record GetActiveMarketEventsQuery(Guid BarmanId) : IRequest<List<MarketEvent>>;
public record GetTemplateMarketEventsQuery(Guid BarmanId) : IRequest<List<MarketEvent>>;
