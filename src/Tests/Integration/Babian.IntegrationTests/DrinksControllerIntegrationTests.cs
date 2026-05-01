using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Babian.IntegrationTests;

[Collection("Integration")]
public class DrinksControllerIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public DrinksControllerIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    // ─── Protection JWT ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetDrinks_WithoutToken_Returns401()
    {
        var client = _fixture.Factory.CreateClient();
        var response = await client.GetAsync("/api/drinks");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDrinks_WithToken_Returns200()
    {
        var userId = Guid.NewGuid();
        var client = AuthHelper.CreateAuthenticatedClient(_fixture.Factory, userId);

        var response = await client.GetAsync("/api/drinks");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ─── Endpoint public ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetPublicDrinks_WithoutToken_Returns200()
    {
        var client = _fixture.Factory.CreateClient();
        var barId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/drinks/public/{barId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ─── Update protégé ───────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateDrink_WithoutToken_Returns401()
    {
        var client = _fixture.Factory.CreateClient();
        var response = await client.PutAsJsonAsync($"/api/drinks/{Guid.NewGuid()}", new
        {
            basePrice = 5m, minPrice = 2m, maxPrice = 10m, category = "Soft"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateDrink_WithTokenAndUnknownId_Returns404()
    {
        var userId = Guid.NewGuid();
        var client = AuthHelper.CreateAuthenticatedClient(_fixture.Factory, userId);

        var response = await client.PutAsJsonAsync($"/api/drinks/{Guid.NewGuid()}", new
        {
            basePrice = 5m, minPrice = 2m, maxPrice = 10m, category = "Soft"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── Delete protégé ───────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteDrink_WithoutToken_Returns401()
    {
        var client = _fixture.Factory.CreateClient();
        var response = await client.DeleteAsync($"/api/drinks/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteDrink_WithTokenAndUnknownId_Returns404()
    {
        var userId = Guid.NewGuid();
        var client = AuthHelper.CreateAuthenticatedClient(_fixture.Factory, userId);
        var response = await client.DeleteAsync($"/api/drinks/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
