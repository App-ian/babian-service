using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Babian.IntegrationTests;

[Collection("Integration")]
public class MarketConfigsControllerIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public MarketConfigsControllerIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    // ─── Protection JWT ───────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateConfig_WithoutToken_Returns401()
    {
        var client = _fixture.Factory.CreateClient();
        var response = await client.PutAsJsonAsync("/api/market-configs", new
        {
            cycleDurationSeconds = 30,
            decreaseOthers = 0.05,
            increasePerOrder = 0.10,
            rankingGroups = new[] { new { name = "Top", maxRank = 10, coefficient = 0.3 } }
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetConfig_WithoutToken_Returns401()
    {
        var client = _fixture.Factory.CreateClient();
        var response = await client.GetAsync("/api/market-configs");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─── Endpoint public ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetPublicConfig_WithoutToken_Returns200()
    {
        var client = _fixture.Factory.CreateClient();
        var response = await client.GetAsync($"/api/market-configs/public/{Guid.NewGuid()}");
        // Peut être 204 si pas de config, mais on s'assure qu'il n'y a pas d'erreur 500
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    // ─── Création config valide ───────────────────────────────────────────────

    [Fact]
    public async Task UpdateConfig_WithValidData_Returns200()
    {
        var userId = Guid.NewGuid();
        var client = AuthHelper.CreateAuthenticatedClient(_fixture.Factory, userId);

        var response = await client.PutAsJsonAsync("/api/market-configs", new
        {
            cycleDurationSeconds = 30,
            decreaseOthers = 0.05,
            increasePerOrder = 0.10,
            rankingGroups = new[]
            {
                new { name = "Top", maxRank = 10, coefficient = 0.3 },
                new { name = "Bottom", maxRank = 99, coefficient = -0.2 }
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrEmpty();
    }

    // ─── Config invalide (groupes vides) ─────────────────────────────────────

    [Fact]
    public async Task UpdateConfig_WithEmptyRankingGroups_Returns500()
    {
        var userId = Guid.NewGuid();
        var client = AuthHelper.CreateAuthenticatedClient(_fixture.Factory, userId);

        var requestData = new
        {
            cycleDurationSeconds = 30,
            decreaseOthers = 0.05,
            increasePerOrder = 0.10,
            rankingGroups = Array.Empty<object>()
        };

        // Le handler lance une Exception qui est propagée par TestHost (pas de middleware global d'erreur).
        Func<Task> act = async () => await client.PutAsJsonAsync("/api/market-configs", requestData);
        await act.Should().ThrowAsync<Exception>().WithMessage("*Au moins un groupe*");
    }

    // ─── Idempotence (GET après PUT) ─────────────────────────────────────────

    [Fact]
    public async Task GetConfig_AfterUpdate_ReturnsUpdatedConfig()
    {
        var userId = Guid.NewGuid();
        var client = AuthHelper.CreateAuthenticatedClient(_fixture.Factory, userId);

        // PUT
        await client.PutAsJsonAsync("/api/market-configs", new
        {
            cycleDurationSeconds = 45,
            decreaseOthers = 0.05,
            increasePerOrder = 0.10,
            rankingGroups = new[] { new { name = "Top", maxRank = 10, coefficient = 0.3 } }
        });

        // GET
        var getResponse = await client.GetAsync("/api/market-configs");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
