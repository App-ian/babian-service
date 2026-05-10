using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Babian.BusinessLayers.GlobalDrinks.Features.GetGlobalDrinks;
using Microsoft.AspNetCore.Authorization;

namespace Babian.Service.Controllers;

[ApiController]
[Route("api/global-drinks")]
public class GlobalDrinksController : AuthorizedControllerBase
{
    private readonly IMediator _mediator;

    public GlobalDrinksController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Liste toutes les boissons du référentiel public. (US 1.3)
    /// </summary>
    /// <returns>Une liste de boisson de référence</returns>
    [HttpGet]
    public async Task<ActionResult<List<GlobalDrinkDto>>> Get()
    {
        var result = await _mediator.Send(new GetGlobalDrinksQuery());
        return Ok(result);
    }

    /// <summary>
    /// Ajoute une nouvelle boisson au référentiel public.
    /// </summary>
    [Authorize(Roles = "SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<Guid>> Post([FromBody] Babian.BusinessLayers.GlobalDrinks.Features.AddGlobalDrink.AddGlobalDrinkCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Met à jour une boisson du référentiel public.
    /// </summary>
    [Authorize(Roles = "SuperAdmin")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(Guid id, [FromBody] Babian.BusinessLayers.GlobalDrinks.Features.UpdateGlobalDrink.UpdateGlobalDrinkCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("L'ID de la route ne correspond pas à l'ID de la boisson.");
        }

        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Supprime une boisson du référentiel public (Hard Delete).
    /// </summary>
    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _mediator.Send(new Babian.BusinessLayers.GlobalDrinks.Features.DeleteGlobalDrink.DeleteGlobalDrinkCommand(id));
        return NoContent();
    }
}
