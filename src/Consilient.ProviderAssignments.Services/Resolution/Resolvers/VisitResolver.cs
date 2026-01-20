using Consilient.Data;
using Consilient.Data.Entities;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Microsoft.Extensions.Logging;

namespace Consilient.ProviderAssignments.Services.Resolution.Resolvers
{
    internal class VisitResolver(IResolutionCache cache, ConsilientDbContext dbContext, ILogger logger)
        : BaseResolver<Visit>(cache, dbContext, logger), IVisitResolver
    {
        protected override IReadOnlyCollection<Visit> LoadEntities(int facilityId, DateOnly date)
        {
            return DbContext.Visits.Where(m => m.DateServiced == date && m.Hospitalization.FacilityId == facilityId).ToList();
        }

        protected override Task<IEnumerable<Visit>?> ResolveRecord(ProviderAssignment record, IReadOnlyCollection<Visit> cachedItems)
        {
            if (!record.ResolvedHospitalizationId.HasValue || record.FacilityId == 0 || !record.ResolvedPatientId.HasValue)
            {
                return Task.FromResult<IEnumerable<Visit>?>(null);
            }
            var items = cachedItems.Where(m => m.HospitalizationId == record.ResolvedHospitalizationId.Value && m.DateServiced == record.ServiceDate);
            return Task.FromResult<IEnumerable<Visit>?>(items);
        }

        protected override void SetResolvedId(ProviderAssignment record, Visit entity)
        {
            record.ResolvedVisitId = entity.Id;
        }
    }
}
