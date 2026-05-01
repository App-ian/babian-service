using MediatR;
using System;

namespace Babian.BusinessLayers.Orders.Features.AddOrder;

public record AddOrderCommand(
    Guid OwnerId, 
    Guid DrinkId, 
    int Quantity, 
    Guid SessionId, 
    int CycleNumber
) : IRequest<Guid>;
