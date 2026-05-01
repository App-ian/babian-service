using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Babian.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Babian.BusinessLayers.Drinks.Features.GetCatalogConfig;

public class GetCatalogConfigQueryHandler : IRequestHandler<GetCatalogConfigQuery, PosCatalogConfigDto>
{
    private readonly AppDbContext _context;

    public GetCatalogConfigQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PosCatalogConfigDto> Handle(GetCatalogConfigQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.Drinks
            .Where(d => d.OwnerId == request.OwnerId && !string.IsNullOrEmpty(d.Plu))
            .OrderBy(d => d.Plu)
            .Select(d => new PosCatalogItemDto(d.Plu!, d.Name, d.IsActive))
            .ToListAsync(cancellationToken);

        return new PosCatalogConfigDto(
            request.RestaurantId,
            new PosCatalogRangeDto("12000", "12050"),
            items
        );
    }
}
