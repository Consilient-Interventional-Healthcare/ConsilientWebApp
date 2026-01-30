using Consilient.Billing.Contracts;
using Consilient.Data;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Billing.Services;

internal class BillingCodeService(ConsilientDbContext dbContext) : IBillingCodeService
{
    public async Task<IEnumerable<BillingCodeDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await dbContext.BillingCodes
            .AsNoTracking()
            .OrderBy(b => b.Code)
            .Select(b => new BillingCodeDto
            {
                Id = b.Id,
                Code = b.Code,
                Description = b.Description
            })
            .ToListAsync(ct);

        return items;
    }
}

