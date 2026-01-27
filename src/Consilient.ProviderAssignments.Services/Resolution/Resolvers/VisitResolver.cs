using Consilient.Data;
using Consilient.Data.Entities.Clinical;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Consilient.ProviderAssignments.Contracts.Validation;
using Microsoft.Extensions.Logging;

namespace Consilient.ProviderAssignments.Services.Resolution.Resolvers;

internal class VisitResolver(IResolutionCache cache, ConsilientDbContext dbContext, ILogger<VisitResolver> logger)
    : BaseResolver<Visit, VisitResolver>(cache, dbContext, logger), IVisitResolver
{
    protected override IReadOnlyCollection<Visit> LoadEntities(int facilityId, DateOnly date)
    {
        return DbContext.Visits
            .Where(v => v.DateServiced == date && v.Hospitalization.FacilityId == facilityId)
            .ToList();
    }

    protected override Task<IEnumerable<Visit>?> ResolveRecord(RowValidationContext ctx, IReadOnlyCollection<Visit> cachedItems)
    {
        // Visit resolution requires both patient and hospitalization to be resolved first
        if (!ctx.Row.ResolvedPatientId.HasValue || !ctx.Row.ResolvedHospitalizationId.HasValue)
        {
            return Task.FromResult<IEnumerable<Visit>?>(null);
        }

        // Find existing visits for this hospitalization on the service date
        var items = cachedItems.Where(v =>
            v.HospitalizationId == ctx.Row.ResolvedHospitalizationId.Value &&
            v.DateServiced == ctx.Row.ServiceDate);

        return Task.FromResult<IEnumerable<Visit>?>(items);
    }

    protected override void SetResolvedId(RowValidationContext ctx, Visit entity)
    {
        ctx.Row.ResolvedVisitId = entity.Id;
        ctx.AddError(ValidationErrorType.Resolution,
            $"This visit has already been imported (Visit ID: {entity.Id})");
    }
}
