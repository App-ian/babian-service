using MediatR;
using System;

namespace Babian.BusinessLayers.MarketEvents.Features.CloseEvent;

public record CloseMarketEventCommand(Guid EventId, Guid BarmanId) : IRequest<bool>;
