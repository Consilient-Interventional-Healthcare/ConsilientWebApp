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

        // Check for duplicate billing entry (same service type + billing code for this visit)
        var duplicateExists = await dataContext.VisitServiceBillings
            .AnyAsync(vsb =>
                vsb.VisitId == request.VisitId &&
                vsb.ServiceTypeId == request.ServiceTypeId &&
                vsb.BillingCodeId == request.BillingCodeId, ct);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                "This service type and billing code combination already exists for this visit.");
        }

        // Validate the pairing is configured in ServiceTypeBillingCodes
        var isValidPairing = await dataContext.ServiceTypeBillingCodes
            .AnyAsync(x => x.ServiceTypeId == request.ServiceTypeId
                        && x.BillingCodeId == request.BillingCodeId, ct);

        if (!isValidPairing)
        {
            throw new InvalidOperationException(
                "This billing code is not valid for the selected service type.");
        }

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
