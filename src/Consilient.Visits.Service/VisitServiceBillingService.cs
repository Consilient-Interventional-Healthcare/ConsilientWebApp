using Consilient.Data;
using Consilient.Data.Entities.Billing;
using Consilient.Visits.Contracts;
using Consilient.Visits.Contracts.Models.Requests;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Visits.Services;

internal class VisitServiceBillingService(ConsilientDbContext dataContext) : IVisitServiceBillingService
{
    public async Task<int> CreateAsync(CreateVisitServiceBillingRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = new VisitServiceBilling
        {
            VisitId = request.VisitId,
            ServiceTypeId = request.ServiceTypeId,
            BillingCodeId = request.BillingCodeId
        };

        dataContext.VisitServiceBillings.Add(entity);
        await dataContext.SaveChangesAsync(ct);

        return entity.Id;
    }

    public async Task<int> DeleteAsync(DeleteVisitServiceBillingRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.VisitServiceBillingId <= 0)
        {
            return 0;
        }

        var affected = await dataContext.VisitServiceBillings
            .Where(vsb => vsb.Id == request.VisitServiceBillingId)
            .ExecuteDeleteAsync(ct);

        return affected;
    }
}
