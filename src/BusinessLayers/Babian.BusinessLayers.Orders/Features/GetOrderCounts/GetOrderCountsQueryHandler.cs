using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using MediatR;

namespace Babian.BusinessLayers.Orders.Features.GetOrderCounts;

public class GetOrderCountsQueryHandler : IRequestHandler<GetOrderCountsQuery, Dictionary<string, int>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderCountsQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    public async Task<Dictionary<string, int>> Handle(GetOrderCountsQuery request, CancellationToken cancellationToken)
    {
        var counts = await _orderRepository.GetCountsBySessionAsync(
            request.SessionId, 
            request.CycleNumber, 
            request.Since, 
            cancellationToken);

        // Map Guid to string for the frontend
        return counts.ToDictionary(x => x.Key.ToString(), x => x.Value);
    }
}
