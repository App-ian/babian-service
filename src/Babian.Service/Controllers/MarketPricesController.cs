using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Babian.BusinessLayers.MarketEngine.Features.GetPosPrices;
using Babian.Service.Filters;

namespace Babian.Service.Controllers;

[ApiController]
[Route("api/market")]
[ApiKeyAuth]
[AllowAnonymous]
public class MarketPricesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MarketPricesController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet("prices")]
    public async Task<ActionResult<PosMarketPricesDto>> GetPrices([FromQuery] string restaurantId)
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdStr == null || !Guid.TryParse(currentUserIdStr, out var currentUserId))
        {
            return Unauthorized("Impossible de vérifier l'identité relative à la clé API.");
        }

        var claimRestaurantId = User.FindFirstValue("RestaurantId");

        if (string.IsNullOrEmpty(restaurantId) || restaurantId != claimRestaurantId)
        {
            return StatusCode(403, "Le restaurantId fourni ne correspond pas à la clé API.");
        }

        var query = new GetPosPricesQuery(currentUserId, restaurantId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
