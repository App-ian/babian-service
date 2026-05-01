using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Babian.BusinessLayers.Drinks.Features.GetCatalogConfig;
using Babian.Service.Filters;

namespace Babian.Service.Controllers;

[ApiController]
[Route("api/catalog")]
[ApiKeyAuth]
[AllowAnonymous] // On désactive le bearer JWT classique au profit de ApiKeyAuth
public class CatalogController : ControllerBase
{
    private readonly IMediator _mediator;

    public CatalogController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet("config")]
    public async Task<ActionResult<PosCatalogConfigDto>> GetConfig([FromQuery] string restaurantId)
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdStr == null || !Guid.TryParse(currentUserIdStr, out var currentUserId))
        {
            return Unauthorized("Impossible de vérifier l'identité associée à la clé API.");
        }

        var claimRestaurantId = User.FindFirstValue("RestaurantId");

        if (string.IsNullOrEmpty(restaurantId) || restaurantId != claimRestaurantId)
        {
            return StatusCode(403, "Le restaurantId fourni ne correspond pas à la clé API.");
        }

        var query = new GetCatalogConfigQuery(currentUserId, restaurantId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
