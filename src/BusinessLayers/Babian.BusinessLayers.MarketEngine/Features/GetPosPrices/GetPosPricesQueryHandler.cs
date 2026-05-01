using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Babian.BusinessLayers.MarketEngine.Features.GetPosPrices;

public class GetPosPricesQueryHandler : IRequestHandler<GetPosPricesQuery, PosMarketPricesDto>
{
    private readonly AppDbContext _context;

    public GetPosPricesQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PosMarketPricesDto> Handle(GetPosPricesQuery request, CancellationToken cancellationToken)
    {
        var prices = await _context.Drinks
            .Where(d => d.OwnerId == request.OwnerId && !string.IsNullOrEmpty(d.Plu) && d.IsActive)
            .OrderBy(d => d.Plu)
            .Select(d => new PosPriceItemDto(
                d.Plu!,
                (int)Math.Round(d.CurrentPrice * 100)
            ))
            .ToListAsync(cancellationToken);

        return new PosMarketPricesDto(
            request.RestaurantId,
            DateTime.UtcNow,
            prices
        );
    }
}
