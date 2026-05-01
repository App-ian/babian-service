using MediatR;
using System;

namespace Babian.BusinessLayers.MarketSessions.Features.StopSession;

public record StopMarketSessionCommand(Guid OwnerId) : IRequest<bool>;
