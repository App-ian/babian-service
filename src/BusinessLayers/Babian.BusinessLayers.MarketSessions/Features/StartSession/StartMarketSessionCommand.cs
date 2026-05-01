using System;
using MediatR;

namespace Babian.BusinessLayers.MarketSessions.Features.StartSession;

public record StartMarketSessionCommand(Guid OwnerId, string Name) : IRequest<Guid>;
