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

[ApiController]
[Route("api/market-sessions")]
public class MarketSessionsController : AuthorizedControllerBase
{
    private readonly IMediator _mediator;

    public MarketSessionsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost("start")]
    public async Task<ActionResult<Guid>> Start(StartMarketSessionCommand command)
    {
        var result = await _mediator.Send(command with { OwnerId = CurrentUserId });
        return Ok(result);
    }

    [HttpPost("stop")]
    public async Task<ActionResult<bool>> Stop(StopMarketSessionCommand command)
    {
        var result = await _mediator.Send(command with { OwnerId = CurrentUserId });
        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<ActionResult<MarketSessionDto>> GetActive()
    {
        var session = await _mediator.Send(new GetActiveSessionQuery(CurrentUserId));
        return session != null ? Ok(session) : NotFound();
    }

    [AllowAnonymous]
    [HttpGet("public/{barId}")]
    public async Task<ActionResult<MarketSessionDto>> GetPublicActive(Guid barId)
    {
        var session = await _mediator.Send(new GetActiveSessionQuery(barId));
        return session != null ? Ok(session) : NotFound();
    }
}
