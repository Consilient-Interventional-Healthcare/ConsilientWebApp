using Consilient.Data;
using Consilient.Data.Entities;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Consilient.ProviderAssignments.Services.Resolution.Resolvers
{
    internal class HospitalizationResolver(IResolutionCache cache, ConsilientDbContext dbContext, ILogger logger)
        : BaseResolver<Hospitalization>(cache, dbContext, logger), IHospitalizationResolver
    {
        protected override IReadOnlyCollection<Hospitalization> LoadEntities(int facilityId, DateOnly date)
        {
            return DbContext.Hospitalizations.AsNoTracking().ToList();
        }

        protected override Task<IEnumerable<Hospitalization>?> ResolveRecord(ProviderAssignment record, IReadOnlyCollection<Hospitalization> cachedItems)
        {
            IEnumerable<Hospitalization>? hospitalizations = null;
            if (record.FacilityId == 0 || !record.ResolvedPatientId.HasValue)
            {
                return Task.FromResult(hospitalizations);
            }
            if (!int.TryParse(record.HospitalNumber, out var caseId))
            {
                return Task.FromResult(hospitalizations);
            }
            hospitalizations = cachedItems.Where(h => h.CaseId == caseId && h.FacilityId == record.FacilityId && h.PatientId == record.ResolvedPatientId.Value).ToList();
            return Task.FromResult<IEnumerable<Hospitalization>?>(hospitalizations);
        }

        protected override void SetResolvedId(ProviderAssignment record, Hospitalization entity)
        {
            record.ResolvedHospitalizationId = entity.Id;
        }
    }
}
