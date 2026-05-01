using Babian.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Babian.IntegrationTests;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;

    public DatabaseFixture()
    {
        _factory = new CustomWebApplicationFactory();
    }

    public CustomWebApplicationFactory Factory => _factory;

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Assure que la base est propre et que le schéma existe
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<DatabaseFixture>
{
}
