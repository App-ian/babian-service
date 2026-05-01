using Babian.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Babian.BusinessLayers.MarketEvents.Features.GetEvents;
using Babian.BusinessLayers.MarketEvents.Features.CreateEvent;
using Babian.BusinessLayers.MarketEvents.Features.DeleteEvent;
using Babian.BusinessLayers.MarketEvents.Features.ActivateEvent;
using Babian.BusinessLayers.MarketEvents.Features.UpdateEvent;
using Babian.BusinessLayers.MarketEvents.Features.CloseEvent;
using Babian.BusinessLayers.MarketEngine.Features.UpdatePrices;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Babian.Service.Controllers;

[Authorize]
[ApiController]
[Route("api/market-events")]
public class MarketEventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MarketEventsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var events = await _mediator.Send(new GetMarketEventsQuery(userId));
        return Ok(events);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var events = await _mediator.Send(new GetActiveMarketEventsQuery(userId));
        return Ok(events);
    }

    [AllowAnonymous]
    [HttpGet("public/{barId}/active")]
    public async Task<IActionResult> GetPublicActive(Guid barId)
    {
        var events = await _mediator.Send(new GetActiveMarketEventsQuery(barId));
        return Ok(events);
    }

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var events = await _mediator.Send(new GetTemplateMarketEventsQuery(userId));
        return Ok(events);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMarketEventCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var commandWithUid = command with { BarmanId = userId };
        var result = await _mediator.Send(commandWithUid);
        return Ok(result);
    }

    [HttpPut("{eventId}")]
    public async Task<IActionResult> Update(Guid eventId, [FromBody] UpdateMarketEventCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // On force l'ID de la route et l'ID utilisateur du token
        command.Id = eventId;
        command.BarmanId = userId;
        
        var success = await _mediator.Send(command);
        return success ? Ok() : NotFound();
    }

    [HttpDelete("{eventId}")]
    public async Task<IActionResult> Delete(Guid eventId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _mediator.Send(new DeleteMarketEventCommand(eventId, userId));
        return success ? Ok() : NotFound();
    }

    [HttpPost("{eventId}/activate")]
    public async Task<IActionResult> Activate(Guid eventId, [FromBody] ActivateEventRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new ActivateMarketEventCommand(eventId, request.DurationMinutes, userId);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpPost("{eventId}/close")]
    public async Task<IActionResult> Close(Guid eventId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _mediator.Send(new CloseMarketEventCommand(eventId, userId));
        return success ? Ok() : NotFound();
    }
}

public record ActivateEventRequest(int DurationMinutes);
