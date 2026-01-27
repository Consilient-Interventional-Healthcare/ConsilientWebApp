using Consilient.Data;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Consilient.ProviderAssignments.Contracts.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Consilient.ProviderAssignments.Services.Resolution.Resolvers;

internal class PatientResolver(IResolutionCache cache, ConsilientDbContext dbContext, ILogger<PatientResolver> logger)
    : BaseResolver<PatientRow, PatientResolver>(cache, dbContext, logger), IPatientResolver
{
    protected override IReadOnlyCollection<PatientRow> LoadEntities(int facilityId, DateOnly date)
    {
        var rows = DbContext.Database.SqlQueryRaw<PatientRow>(@"
                SELECT
                   P.Id AS PatientId
                  ,BirthDate AS PatientDob
                  ,FirstName AS PatientFirstName
                  ,LastName AS PatientLastName
                  ,PF.MRN AS PatientMrn
                  ,PF.FacilityID AS FacilityId
                FROM Clinical.Patients AS P
                LEFT JOIN Clinical.PatientFacilities as PF ON P.ID = PF.PatientID
            ").ToList();
        return rows;
    }

    protected override Task<IEnumerable<PatientRow>?> ResolveRecord(RowValidationContext ctx, IReadOnlyCollection<PatientRow> cachedItems)
    {
        if (ctx.Row.FacilityId == 0)
        {
            return Task.FromResult<IEnumerable<PatientRow>?>(null);
        }
        if (!string.IsNullOrEmpty(ctx.Row.Mrn))
        {
            var matchedByMrn = cachedItems.Where(p => p.PatientMrn == ctx.Row.Mrn && p.FacilityId.HasValue && p.FacilityId.Value == ctx.Row.FacilityId).ToList();
            if (matchedByMrn.Count != 0)
            {
                return Task.FromResult<IEnumerable<PatientRow>?>(matchedByMrn);
            }
        }
        return Task.FromResult<IEnumerable<PatientRow>?>(null);
    }

    protected override void SetResolvedId(RowValidationContext ctx, PatientRow entity)
    {
        ctx.Row.ResolvedPatientId = entity.PatientId;
    }
}
