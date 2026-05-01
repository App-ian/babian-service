using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Babian.BusinessLayers.MarketConfigs.Features.UpdateConfig;
using Babian.BusinessLayers.MarketConfigs.Features.GetConfig;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Babian.Service.Controllers;

[Authorize]
[ApiController]
[Route("api/market-configs")]
public class MarketConfigsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MarketConfigsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Met à jour ou crée la configuration globale de marché pour un barman. (US 1.1)
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateConfig([FromBody] UpdateMarketConfigCommand command)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var commandWithUserId = command with { BarmanId = currentUserId };
        
        var resultId = await _mediator.Send(commandWithUserId);
        return Ok(new { MarketConfigId = resultId });
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var config = await _mediator.Send(new GetMarketConfigQuery(currentUserId));
        return Ok(config);
    }

    [AllowAnonymous]
    [HttpGet("public/{barId}")]
    public async Task<IActionResult> GetPublic(Guid barId)
    {
        var config = await _mediator.Send(new GetMarketConfigQuery(barId));
        return Ok(config);
    }
}
