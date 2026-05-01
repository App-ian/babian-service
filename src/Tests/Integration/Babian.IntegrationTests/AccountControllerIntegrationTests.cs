using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Babian.IntegrationTests;

[Collection("Integration")]
public class AccountControllerIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly HttpClient _client;

    public AccountControllerIntegrationTests(DatabaseFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    // ─── Login ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/account/login", new
        {
            email = "nobody@example.com",
            password = "wrong"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─── Protected endpoints ─────────────────────────────────────────────────

    [Fact]
    public async Task GetMe_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/account/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_WithValidData_Returns200()
    {
        var response = await _client.PostAsJsonAsync("/api/account/register", new
        {
            email = $"test_{Guid.NewGuid():N}@babian.com",
            password = "StrongPass123!",
            firstName = "Test",
            lastName = "User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrEmpty();
    }
}
