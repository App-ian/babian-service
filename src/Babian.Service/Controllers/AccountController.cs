using System.Threading.Tasks;
using Babian.BusinessLayers.Profiles;
using Babian.BusinessLayers.Profiles.Features.Login;
using Babian.BusinessLayers.Profiles.Features.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Babian.Domain.Interfaces;
using System.Security.Claims;

namespace Babian.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserRepository _userRepository;

    public AccountController(IMediator mediator, IUserRepository userRepository)
    {
        _mediator = mediator;
        _userRepository = userRepository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var userId = await _mediator.Send(new RegisterUserCommand(
            request.Email, 
            request.Password, 
            request.FirstName, 
            request.LastName,
            request.Role ?? "Barman"));
            
        return Ok(new { id = userId });
    }

    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<AuthResponse>> GetCurrentUserInfo()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userRepository.GetByEmailAsync(email!, default);
        
        if (string.IsNullOrEmpty(user!.ApiKey))
        {
            user.ApiKey = "babian_pos_" + System.Guid.NewGuid().ToString("N");
            await _userRepository.UpdateAsync(user, default);
            await _userRepository.SaveChangesAsync(default);
        }

        return Ok(new AuthResponse(
            string.Empty,
            user!.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.Id,
            user.RestaurantId,
            user.ApiKey
        ));
    }

    [HttpPut("profile")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] Babian.BusinessLayers.Profiles.Features.UpdateProfile.UpdateUserProfileCommand command)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userRepository.GetByEmailAsync(email!, default);
        
        if (user == null) return NotFound();

        // Enforce setting the correct UserId from the authenticated token
        var secureCommand = command with { UserId = user.Id };
        
        await _mediator.Send(secureCommand);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var result = await _mediator.Send(new LoginUserQuery(request.Email, request.Password));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
