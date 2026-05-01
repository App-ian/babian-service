using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Babian.IntegrationTests;

[Collection("Integration")]
public class MarketSessionsControllerIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public MarketSessionsControllerIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    // ─── Protection JWT ───────────────────────────────────────────────────────

    [Fact]
    public async Task StartSession_WithoutToken_Returns401()
    {
        var client = _fixture.Factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/market-sessions/start", new { name = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetActive_WithoutToken_Returns401()
    {
        var client = _fixture.Factory.CreateClient();
        var response = await client.GetAsync("/api/market-sessions/active");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─── Endpoint public ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetPublicActive_WithoutToken_Returns200Or404()
    {
        var client = _fixture.Factory.CreateClient();
        var response = await client.GetAsync($"/api/market-sessions/public/{Guid.NewGuid()}");

        var validStatuses = new[] { HttpStatusCode.OK, HttpStatusCode.NotFound };
        validStatuses.Should().Contain(response.StatusCode);
    }

    // ─── Start session ────────────────────────────────────────────────────────

    [Fact]
    public async Task StartSession_WithToken_Returns200AndSessionId()
    {
        var userId = Guid.NewGuid();
        var client = AuthHelper.CreateAuthenticatedClient(_fixture.Factory, userId);

        var response = await client.PostAsJsonAsync("/api/market-sessions/start", new { name = "Soirée Test" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task StartSession_CalledTwice_IsIdempotent()
    {
        var userId = Guid.NewGuid();
        var client = AuthHelper.CreateAuthenticatedClient(_fixture.Factory, userId);

        var response1 = await client.PostAsJsonAsync("/api/market-sessions/start", new { name = "Session 1" });
        var response2 = await client.PostAsJsonAsync("/api/market-sessions/start", new { name = "Session 2" });

        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        var id1 = await response1.Content.ReadFromJsonAsync<Guid>();
        var id2 = await response2.Content.ReadFromJsonAsync<Guid>();
        id1.Should().Be(id2, "le 2ème appel doit retourner la session existante");
    }

    // ─── Stop session ─────────────────────────────────────────────────────────

    [Fact]
    public async Task StopSession_WithToken_Returns200()
    {
        var userId = Guid.NewGuid();
        var client = AuthHelper.CreateAuthenticatedClient(_fixture.Factory, userId);

        // Démarrer une session d'abord
        await client.PostAsJsonAsync("/api/market-sessions/start", new { name = "À arrêter" });

        var response = await client.PostAsJsonAsync("/api/market-sessions/stop", new { });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ─── GetActive ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetActive_WhenSessionExists_Returns200()
    {
        var userId = Guid.NewGuid();
        var client = AuthHelper.CreateAuthenticatedClient(_fixture.Factory, userId);

        await client.PostAsJsonAsync("/api/market-sessions/start", new { name = "Active" });

        var response = await client.GetAsync("/api/market-sessions/active");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetActive_WhenNoSession_Returns404()
    {
        var userId = Guid.NewGuid();
        var client = AuthHelper.CreateAuthenticatedClient(_fixture.Factory, userId);
        // Cet userId n'a jamais créé de session
        var response = await client.GetAsync("/api/market-sessions/active");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
