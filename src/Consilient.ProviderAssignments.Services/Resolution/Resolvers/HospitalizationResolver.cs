using Consilient.Data;
using Consilient.Data.Entities.Clinical;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Consilient.ProviderAssignments.Contracts.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Consilient.ProviderAssignments.Services.Resolution.Resolvers;

internal class HospitalizationResolver(IResolutionCache cache, ConsilientDbContext dbContext, ILogger<HospitalizationResolver> logger)
    : BaseResolver<Hospitalization, HospitalizationResolver>(cache, dbContext, logger), IHospitalizationResolver
{
    protected override IReadOnlyCollection<Hospitalization> LoadEntities(int facilityId, DateOnly date)
    {
        return DbContext.Hospitalizations.AsNoTracking().ToList();
    }

    protected override Task<IEnumerable<Hospitalization>?> ResolveRecord(RowValidationContext ctx, IReadOnlyCollection<Hospitalization> cachedItems)
    {
        IEnumerable<Hospitalization>? hospitalizations = null;
        if (ctx.Row.FacilityId == 0 || !ctx.Row.ResolvedPatientId.HasValue)
        {
            return Task.FromResult(hospitalizations);
        }
        if (!int.TryParse(ctx.Row.HospitalNumber, out var caseId))
        {
            return Task.FromResult(hospitalizations);
        }
        hospitalizations = cachedItems.Where(h => h.CaseId == caseId && h.FacilityId == ctx.Row.FacilityId && h.PatientId == ctx.Row.ResolvedPatientId.Value).ToList();
        return Task.FromResult<IEnumerable<Hospitalization>?>(hospitalizations);
    }

    protected override void SetResolvedId(RowValidationContext ctx, Hospitalization entity)
    {
        ctx.Row.ResolvedHospitalizationId = entity.Id;
    }
}
