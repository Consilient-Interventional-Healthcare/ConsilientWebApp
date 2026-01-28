using Consilient.Common;
using Consilient.Data;
using Consilient.Data.Entities.Clinical;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Consilient.ProviderAssignments.Contracts.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Consilient.ProviderAssignments.Services.Resolution.Resolvers;

/// <summary>
/// Resolves hospitalization status from provider assignments.
/// Sets status to 1 when PsychEval is null or empty.
/// </summary>
internal class HospitalizationStatusResolver(IResolutionCache cache, ConsilientDbContext dbContext, ILogger<HospitalizationStatusResolver> logger)
    : BaseResolver<HospitalizationStatusEntity, HospitalizationStatusResolver>(cache, dbContext, logger), IHospitalizationStatusResolver
{
    protected override IReadOnlyCollection<HospitalizationStatusEntity> LoadEntities(int facilityId, DateOnly date)
    {
        // Load all hospitalization statuses - they're reference data and should be cached
        return DbContext.HospitalizationStatuses.AsNoTracking().ToList();
    }

    protected override Task<IEnumerable<HospitalizationStatusEntity>?> ResolveRecord(RowValidationContext ctx, IReadOnlyCollection<HospitalizationStatusEntity> cachedItems)
    {
        IEnumerable<HospitalizationStatusEntity>? statuses = null;

        // If PsychEval is null or empty, resolve to status ID 1
        if (!ctx.Row.ResolvedHospitalizationStatus.HasValue && string.IsNullOrWhiteSpace(ctx.Row.PsychEval))
        {
            statuses = cachedItems.Where(s => s.Id == 1).ToList();
        }

        return Task.FromResult(statuses);
    }

    protected override void SetResolvedId(RowValidationContext ctx, HospitalizationStatusEntity entity)
    {
        ctx.Row.ResolvedHospitalizationStatus = (HospitalizationStatus)entity.Id;
    }
}
