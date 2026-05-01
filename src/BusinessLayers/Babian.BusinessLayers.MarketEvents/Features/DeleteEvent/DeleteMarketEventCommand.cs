using MediatR;
using System;

namespace Babian.BusinessLayers.MarketEvents.Features.DeleteEvent;

public record DeleteMarketEventCommand(Guid EventId, Guid BarmanId) : IRequest<bool>;
