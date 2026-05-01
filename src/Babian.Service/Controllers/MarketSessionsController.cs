using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Babian.BusinessLayers.MarketSessions.Features.StartSession;
using Babian.BusinessLayers.MarketSessions.Features.StopSession;
using Babian.BusinessLayers.MarketSessions.Features.GetActiveSession;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Babian.Service.Controllers;

[Authorize]
[ApiController]
[Route("api/market-sessions")]
public class MarketSessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MarketSessionsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost("start")]
    public async Task<ActionResult<Guid>> Start(StartMarketSessionCommand command)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var commandWithUserId = command with { OwnerId = currentUserId };
        
        var result = await _mediator.Send(commandWithUserId);
        return Ok(result);
    }

    [HttpPost("stop")]
    public async Task<ActionResult<bool>> Stop(StopMarketSessionCommand command)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var commandWithUserId = command with { OwnerId = currentUserId };
        
        var result = await _mediator.Send(commandWithUserId);
        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var session = await _mediator.Send(new GetActiveSessionQuery(currentUserId));
        return session != null ? Ok(session) : NotFound();
    }

    [AllowAnonymous]
    [HttpGet("public/{barId}")]
    public async Task<IActionResult> GetPublicActive(Guid barId)
    {
        var session = await _mediator.Send(new GetActiveSessionQuery(barId));
        return session != null ? Ok(session) : NotFound();
    }
}
