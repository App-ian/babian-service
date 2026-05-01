using Babian.Domain.Entities;
using MediatR;
using System;

namespace Babian.BusinessLayers.MarketConfigs.Features.GetConfig;

public record GetMarketConfigQuery(Guid OwnerId) : IRequest<MarketConfig?>;
