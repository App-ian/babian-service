using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Babian.IntegrationTests;

[Collection("Integration")]
public class MarketEngineControllerIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public MarketEngineControllerIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    // ─── Simulate (AllowAnonymous) ───────────────────────────────────────────

    [Fact]
    public async Task Simulate_WithoutToken_Returns200()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/market-engine/simulate", new
        {
            currentPrice = 5.0,
            minPrice = 2.0,
            maxPrice = 10.0,
            rank = 1,
            groups = new[]
            {
                new { name = "Top", maxRank = 10, coefficient = 0.3 }
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var price = await response.Content.ReadFromJsonAsync<decimal>();
        price.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Simulate_WithEmptyGroups_UsesDefaultFallback()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/market-engine/simulate", new
        {
            currentPrice = 5.0,
            minPrice = 2.0,
            maxPrice = 10.0,
            rank = 1,
            groups = Array.Empty<object>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ─── Cycle (Authorize) ────────────────────────────────────────────────────

    [Fact]
    public async Task TriggerCycle_WithoutToken_Returns401()
    {
        var client = _fixture.Factory.CreateClient();
        var response = await client.PostAsync("/api/market-engine/cycle", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TriggerCycle_WithTokenAndNoSession_Returns200WithFalse()
    {
        var userId = Guid.NewGuid();
        var client = AuthHelper.CreateAuthenticatedClient(_fixture.Factory, userId);

        var response = await client.PostAsync("/api/market-engine/cycle", null);

        // Pas de session active → handler retourne false, le controller retourne Ok(false)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<bool>();
        result.Should().BeFalse();
    }

    // ─── GetLastVariations (AllowAnonymous) ───────────────────────────────────

    [Fact]
    public async Task GetLastVariations_WithoutToken_Returns200()
    {
        var client = _fixture.Factory.CreateClient();
        var sessionId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/market-engine/history/{sessionId}/last");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
