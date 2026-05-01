using MediatR;
using System;

namespace Babian.BusinessLayers.MarketEngine.Features.UpdatePrices;

public record UpdateMarketPricesCommand(Guid OwnerId, bool IncrementCycle = true, bool IsManualForce = false) : IRequest<bool>;
