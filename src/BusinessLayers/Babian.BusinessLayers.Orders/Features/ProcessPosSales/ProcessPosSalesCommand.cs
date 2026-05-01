using System;
using MediatR;

namespace Babian.BusinessLayers.Orders.Features.ProcessPosSales;

public record ProcessPosSalesCommand(
    string RestaurantId,
    string Plu,
    int Quantity,
    DateTime Timestamp,
    Guid OwnerId
) : IRequest<bool>;
