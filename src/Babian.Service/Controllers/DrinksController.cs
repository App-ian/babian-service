using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Babian.BusinessLayers.Drinks.Features.ActivateDrink;
using Babian.BusinessLayers.Drinks.Features.GetDrinks;
using Babian.BusinessLayers.Drinks.Features.UpdateDrink;
using Babian.BusinessLayers.Drinks.Features.DeactivateDrink;
using Babian.BusinessLayers.Drinks.Features.UpdatePrice;
using Microsoft.AspNetCore.Authorization;

namespace Babian.Service.Controllers;

[ApiController]
[Route("api/drinks")]
public class DrinksController : AuthorizedControllerBase
{
    private readonly IMediator _mediator;

    public DrinksController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Active une boisson du référentiel global pour le barman courant. (US 1.3)
    /// </summary>
    /// <param name="command">Détails de l'activation (Id global, prix min/max)</param>
    /// <returns>L'Id de la boisson créée dans le catalogue du barman</returns>
    [HttpPost("activate")]
    public async Task<IActionResult> Activate([FromBody] ActivateDrinkFromGlobalCommand command)
    {
        var commandWithUserId = command with { BarmanId = CurrentUserId };
        var resultId = await _mediator.Send(commandWithUserId);
        return Ok(new { DrinkId = resultId });
    }

    /// <summary>
    /// Récupère la liste des boissons activées pour un barman.
    /// </summary>
    /// <returns>Liste des boissons avec leurs prix actuels</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DrinkDto>>> GetByOwner()
    {
        var drinks = await _mediator.Send(new GetDrinksByOwnerQuery(CurrentUserId));
        return Ok(drinks);
    }

    [AllowAnonymous]
    [HttpGet("public/{barId}")]
    public async Task<ActionResult<IEnumerable<DrinkDto>>> GetPublicByOwner(Guid barId)
    {
        var drinks = await _mediator.Send(new GetDrinksByOwnerQuery(barId));
        return Ok(drinks);
    }

    /// <summary>
    /// Met à jour les paramètres d'une boisson (prix base, min, max) pour le barman courant.
    /// </summary>
    [HttpPut("{drinkId}")]
    public async Task<IActionResult> Update(Guid drinkId, [FromBody] UpdateDrinkCommand command)
    {
        var commandWithIds = command with { DrinkId = drinkId, BarmanId = CurrentUserId };
        var success = await _mediator.Send(commandWithIds);
        return success ? Ok() : NotFound();
    }

    /// <summary>
    /// Désactive une boisson du catalogue du barman.
    /// </summary>
    [HttpDelete("{drinkId}")]
    public async Task<IActionResult> Deactivate(Guid drinkId)
    {
        var success = await _mediator.Send(new DeactivateDrinkCommand(drinkId, CurrentUserId));
        return success ? Ok() : NotFound();
    }

    /// <summary>
    /// Met à jour manuellement le prix boursier d'une boisson.
    /// </summary>
    [HttpPost("{drinkId}/price")]
    public async Task<IActionResult> UpdatePrice(Guid drinkId, [FromBody] UpdatePriceRequest request)
    {
        var success = await _mediator.Send(new UpdateDrinkPriceCommand(drinkId, CurrentUserId, request.Price));
        return success ? Ok() : NotFound();
    }
}

public record UpdatePriceRequest(decimal Price);
