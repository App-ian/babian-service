using System;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using Babian.BusinessLayers.Profiles.Services;
using MediatR;
using BC = BCrypt.Net.BCrypt;

namespace Babian.BusinessLayers.Profiles.Features.Login;

public record LoginUserQuery(string Email, string Password) : IRequest<AuthResponse>;

public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public LoginUserQueryHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> Handle(LoginUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null || !BC.Verify(request.Password, user.PasswordHash))
        {
            throw new Exception("Email ou mot de passe incorrect.");
        }

        var token = _jwtService.GenerateToken(user);

        return new AuthResponse(
            token,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.Id,
            user.RestaurantId
        );
    }
}
