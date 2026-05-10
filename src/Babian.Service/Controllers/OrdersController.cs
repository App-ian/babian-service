using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Babian.BusinessLayers.Orders.Features.AddOrder;
using Babian.BusinessLayers.Orders.Features.GetOrderCounts;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;

namespace Babian.Service.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : AuthorizedControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Add(AddOrderCommand command)
    {
        var result = await _mediator.Send(command with { OwnerId = CurrentUserId });
        return Ok(result);
    }

    [HttpGet("counts")]
    public async Task<ActionResult<Dictionary<string, int>>> GetCounts(
        [FromQuery] Guid sessionId, 
        [FromQuery] int? cycleNumber, 
        [FromQuery] DateTime? since)
    {
        var query = new GetOrderCountsQuery(sessionId, cycleNumber, since);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
