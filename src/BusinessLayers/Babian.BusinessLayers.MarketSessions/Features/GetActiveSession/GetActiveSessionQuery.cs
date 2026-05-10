using MediatR;
using System;

namespace Babian.BusinessLayers.MarketSessions.Features.GetActiveSession;

public record GetActiveSessionQuery(Guid OwnerId) : IRequest<MarketSessionDto?>;
