using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using Babian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Babian.Infrastructure.Persistence.Repositories;

public class DrinkRepository : IDrinkRepository
{
    private readonly AppDbContext _context;

    public DrinkRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<GlobalDrink?> GetGlobalDrinkByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.GlobalDrinks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Drink?> GetByOwnerAndGlobalIdAsync(Guid ownerId, Guid globalDrinkId, CancellationToken cancellationToken)
    {
        return await _context.Drinks
            .FirstOrDefaultAsync(x => x.OwnerId == ownerId && x.GlobalDrinkId == globalDrinkId, cancellationToken);
    }

    public async Task AddAsync(Drink drink, CancellationToken cancellationToken)
    {
        await _context.Drinks.AddAsync(drink, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Drink drink, CancellationToken cancellationToken)
    {
        _context.Drinks.Update(drink);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            _context.Entry(drink).State = EntityState.Detached;
            throw;
        }
    }

    public async Task<Drink?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Drinks
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Drink>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        return await _context.Drinks
            .Where(x => x.OwnerId == ownerId && x.IsActive)
            .ToListAsync(cancellationToken);
    }
}
