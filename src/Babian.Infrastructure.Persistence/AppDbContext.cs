using Babian.Domain.Entities;
using Babian.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Babian.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<MarketConfig> MarketConfigs => Set<MarketConfig>();
    public DbSet<Drink> Drinks => Set<Drink>();
    public DbSet<GlobalDrink> GlobalDrinks => Set<GlobalDrink>();
    public DbSet<MarketSession> MarketSessions => Set<MarketSession>();
    public DbSet<PriceHistory> PriceHistories => Set<PriceHistory>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<User> Users => Set<User>();
    public DbSet<MarketEvent> MarketEvents => Set<MarketEvent>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Explicitly apply MarketEventConfiguration to avoid naming issues
        modelBuilder.ApplyConfiguration(new MarketEventConfiguration());
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
