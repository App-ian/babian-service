using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Babian.Domain.Entities;
using Babian.Infrastructure.Persistence;
using Babian.BusinessLayers.Drinks.Features.GetCatalogConfig;
using Babian.BusinessLayers.MarketEngine.Features.GetPosPrices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Babian.IntegrationTests;

[Collection("Integration")]
public class PosEndpointsIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public PosEndpointsIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<(string ApiKey, string RestaurantId)> SetupUserAndDrinksAsync()
    {
        var apiKey = Guid.NewGuid().ToString();
        var restaurantId = "rest-" + Guid.NewGuid().ToString().Substring(0, 5);
        var userId = Guid.NewGuid();
        
        using var scope = _fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var user = new User { Id = userId, ApiKey = apiKey, RestaurantId = restaurantId, Email = $"pos-{userId}@test.com" };
        var globalDrink = new GlobalDrink { Id = Guid.NewGuid(), Name = "Test Drink", DefaultBasePrice = 5, Plu = "12001" };
        var drink = new Drink { Id = Guid.NewGuid(), OwnerId = userId, Plu = "12001", Name = "Test Drink", MinPrice = 4, MaxPrice = 8, CurrentPrice = 6.50m, GlobalDrinkId = globalDrink.Id, IsActive = true };
        var session = new MarketSession { Id = Guid.NewGuid(), OwnerId = userId, IsActive = true, Name = "Session POS Test" };
        
        db.Users.Add(user);
        db.GlobalDrinks.Add(globalDrink);
        db.Drinks.Add(drink);
        db.MarketSessions.Add(session);
        await db.SaveChangesAsync();

        return (apiKey, restaurantId);
    }

    [Fact]
    public async Task ProcessSales_WithValidApiKey_Returns202()
    {
        var setup = await SetupUserAndDrinksAsync();
        var client = _fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", setup.ApiKey);

        var payload = new { restaurantId = setup.RestaurantId, plu = "12001", quantity = 2, timestamp = DateTime.UtcNow };
        var response = await client.PostAsJsonAsync("/api/webhooks/sales", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task GetCatalogConfig_WithValidApiKey_ReturnsData()
    {
        var setup = await SetupUserAndDrinksAsync();
        var client = _fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", setup.ApiKey);

        var response = await client.GetAsync($"/api/catalog/config?restaurantId={setup.RestaurantId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await response.Content.ReadFromJsonAsync<PosCatalogConfigDto>();
        data.Should().NotBeNull();
        data!.RestaurantId.Should().Be(setup.RestaurantId);
        data.Items.Should().ContainSingle(i => i.Plu == "12001");
    }

    [Fact]
    public async Task GetMarketPrices_WithValidApiKey_ReturnsConvertedPrices()
    {
        var setup = await SetupUserAndDrinksAsync();
        var client = _fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", setup.ApiKey);

        var response = await client.GetAsync($"/api/market/prices?restaurantId={setup.RestaurantId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await response.Content.ReadFromJsonAsync<PosMarketPricesDto>();
        data.Should().NotBeNull();
        data!.RestaurantId.Should().Be(setup.RestaurantId);
        data.Prices.Should().ContainSingle();
        data.Prices[0].PriceCents.Should().Be(650); // 6.50m => 650
    }

    [Fact]
    public async Task AnyEndpoint_WithoutApiKey_Returns401()
    {
        var client = _fixture.Factory.CreateClient();
        
        var response1 = await client.GetAsync("/api/catalog/config?restaurantId=any");
        response1.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var response2 = await client.GetAsync("/api/market/prices?restaurantId=any");
        response2.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var response3 = await client.PostAsJsonAsync("/api/webhooks/sales", new { restaurantId = "any", plu = "any", quantity = 1, timestamp = DateTime.UtcNow });
        response3.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
