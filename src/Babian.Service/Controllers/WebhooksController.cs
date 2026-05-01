using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Babian.BusinessLayers.Orders.Features.ProcessPosSales;
using Babian.Service.Filters;

namespace Babian.Service.Controllers;

[ApiController]
[Route("api/webhooks")]
[ApiKeyAuth]
[AllowAnonymous]
public class WebhooksController : ControllerBase
{
    private readonly IMediator _mediator;

    public WebhooksController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public record PosSalesPayload(string restaurantId, string plu, int quantity, DateTime timestamp);

    [HttpPost("sales")]
    public async Task<IActionResult> ProcessSales([FromBody] PosSalesPayload payload)
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdStr == null || !Guid.TryParse(currentUserIdStr, out var currentUserId))
        {
            return Unauthorized(new { status = "error", message = "Identité clé API non valide." });
        }

        var claimRestaurantId = User.FindFirstValue("RestaurantId");

        if (string.IsNullOrEmpty(payload.restaurantId) || payload.restaurantId != claimRestaurantId)
        {
            return StatusCode(403, new { status = "error", message = "Le restaurantId fournit dans le body est différent de celui de l'API Key." });
        }

        try
        {
            var command = new ProcessPosSalesCommand(
                payload.restaurantId,
                payload.plu,
                payload.quantity,
                payload.timestamp,
                currentUserId
            );
            
            await _mediator.Send(command);

            return Accepted(new { status = "success", message = "Vente enregistrée avec succès." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { status = "error", message = ex.Message });
        }
    }
}
