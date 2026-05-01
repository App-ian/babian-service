using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;

namespace Babian.IntegrationTests;

public static class AuthHelper
{
    private const string SecretKey = "vibe_code_babian_secret_key_extremely_long_and_secure_2026";
    private const string Issuer = "BabianAPI";
    private const string Audience = "BabianFrontend";

    public static string GenerateToken(Guid userId, string email, string role = "Barman")
    {
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static HttpClient CreateAuthenticatedClient(
        CustomWebApplicationFactory factory,
        Guid userId,
        string email = "test@babian.com")
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Babian.Infrastructure.Persistence.AppDbContext>();
        
        if (!System.Linq.Enumerable.Any(db.Users, u => u.Id == userId))
        {
            db.Users.Add(new Babian.Domain.Entities.User 
            { 
                Id = userId, 
                Email = email, 
                Role = "Barman",
                FirstName = "Test",
                LastName = "User",
                RestaurantId = "rest-test"
            });
            db.SaveChanges();
        }

        var client = factory.CreateClient();
        var token = GenerateToken(userId, email);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
