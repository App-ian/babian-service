using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Babian.BusinessLayers.MarketEngine.Features.UpdatePrices;
using Babian.BusinessLayers.MarketEngine.Features.GetHistory;

namespace Babian.Service.Controllers;

[ApiController]
[Route("api/market-engine")]
public class MarketEngineController : AuthorizedControllerBase
{
    private readonly IMediator _mediator;
    private readonly Babian.Domain.Interfaces.IMarketSimulationService _simulationService;

    public MarketEngineController(IMediator mediator, Babian.Domain.Interfaces.IMarketSimulationService simulationService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _simulationService = simulationService ?? throw new ArgumentNullException(nameof(simulationService));
    }

    /// <summary>
    /// Déclenche manuellement un cycle de calcul des prix (US 2.1).
    /// </summary>
    [HttpPost("cycle")]
    public async Task<ActionResult<bool>> TriggerCycle([FromQuery] bool force = false)
    {
        var result = await _mediator.Send(new UpdateMarketPricesCommand(CurrentUserId, true, force));
        return Ok(result);
    }

    /// <summary>
    /// Simule un calcul de prix sans impacter la base de données (Utile pour les tests).
    /// </summary>
    [AllowAnonymous]
    [HttpPost("simulate")]
    public ActionResult<decimal> SimulatePrice([FromBody] Babian.BusinessLayers.MarketEngine.Features.SimulatePrice.SimulatePriceRequest request)
    {
        var groups = request.Groups?.Select(g => new Babian.Domain.Entities.RankingGroup 
        { 
            Name = g.Name, 
            MaxRank = g.MaxRank, 
            Coefficient = g.Coefficient 
        }).ToList();

        var result = _simulationService.SimulateNextPrice(
            request.CurrentPrice, 
            request.MinPrice, 
            request.MaxPrice, 
            request.Rank, 
            groups);

        return Ok(result);
    }

    /// <summary>
    /// Récupère les dernières variations de prix pour une session (Public).
    /// </summary>
    [AllowAnonymous]
    [HttpGet("history/{sessionId}/last")]
    public async Task<ActionResult<List<PriceVariationDto>>> GetLastVariations(Guid sessionId)
    {
        var result = await _mediator.Send(new GetLastPriceVariationsQuery(sessionId));
        return Ok(result);
    }
}
