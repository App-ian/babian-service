using MediatR;
using System;
using System.Collections.Generic;

namespace Babian.BusinessLayers.Orders.Features.GetOrderCounts;

public record GetOrderCountsQuery(
    Guid SessionId,
    int? CycleNumber = null,
    DateTime? Since = null
) : IRequest<Dictionary<string, int>>;
