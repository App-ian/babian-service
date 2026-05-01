using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Babian.Domain.Interfaces;
using Babian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Babian.Infrastructure.Persistence.Repositories;

public class GlobalDrinkRepository : IGlobalDrinkRepository
{
    private readonly AppDbContext _context;

    public GlobalDrinkRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<GlobalDrink>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.GlobalDrinks
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<GlobalDrink?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.GlobalDrinks
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(GlobalDrink globalDrink, CancellationToken cancellationToken)
    {
        await _context.GlobalDrinks.AddAsync(globalDrink, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(GlobalDrink globalDrink, CancellationToken cancellationToken)
    {
        _context.GlobalDrinks.Update(globalDrink);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(GlobalDrink globalDrink, CancellationToken cancellationToken)
    {
        _context.GlobalDrinks.Remove(globalDrink);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
