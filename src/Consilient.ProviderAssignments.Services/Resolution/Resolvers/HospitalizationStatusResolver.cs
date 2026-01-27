using Consilient.Data;
using Consilient.Data.Entities.Clinical;
using Consilient.Data.Entities.Staging;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Consilient.ProviderAssignments.Services.Resolution.Resolvers;

/// <summary>
/// Resolves hospitalization status from provider assignments.
/// Sets status to 1 when PsychEval is null or empty.
/// </summary>
internal class HospitalizationStatusResolver(IResolutionCache cache, ConsilientDbContext dbContext, ILogger<HospitalizationStatusResolver> logger)
    : BaseResolver<HospitalizationStatus, HospitalizationStatusResolver>(cache, dbContext, logger), IHospitalizationStatusResolver
{
    protected override IReadOnlyCollection<HospitalizationStatus> LoadEntities(int facilityId, DateOnly date)
    {
        // Load all hospitalization statuses - they're reference data and should be cached
        return DbContext.HospitalizationStatuses.AsNoTracking().ToList();
    }

    protected override Task<IEnumerable<HospitalizationStatus>?> ResolveRecord(ProviderAssignment record, IReadOnlyCollection<HospitalizationStatus> cachedItems)
    {
        IEnumerable<HospitalizationStatus>? statuses = null;

        // If PsychEval is null or empty, resolve to status ID 1
        if (!record.ResolvedHospitalizationStatusId.HasValue && string.IsNullOrWhiteSpace(record.PsychEval))
        {
            statuses = cachedItems.Where(s => s.Id == 1).ToList();
        }

        return Task.FromResult(statuses);
    }

    protected override void SetResolvedId(ProviderAssignment record, HospitalizationStatus entity)
    {
        record.ResolvedHospitalizationStatusId = entity.Id;
    }
}
