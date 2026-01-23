using Consilient.Common;
using Consilient.Data;
using Consilient.Data.Entities.Staging;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Consilient.ProviderAssignments.Services.Resolution.Resolvers
{
    /// <summary>
    /// Abstract base class for provider resolution with shared lookup/matching logic.
    /// </summary>
    internal abstract class ProviderResolver<TResolver>(IResolutionCache cache, ConsilientDbContext dbContext, ILogger<TResolver> logger)
        : BaseResolver<ProviderRow, TResolver>(cache, dbContext, logger)
    {
        /// <summary>
        /// The provider type this resolver targets (Physician or NursePractitioner).
        /// </summary>
        protected abstract ProviderType TargetProviderType { get; }

        /// <summary>
        /// Gets the last name from the record to match against providers.
        /// </summary>
        protected abstract string GetLastNameFromRecord(ProviderAssignment record);

        /// <summary>
        /// Sets the resolved provider ID on the record.
        /// </summary>
        protected abstract void SetResolvedProviderIdOnRecord(ProviderAssignment record, int providerId);

        protected override IReadOnlyCollection<ProviderRow> LoadEntities(int facilityId, DateOnly date)
        {
            return DbContext.Database.SqlQueryRaw<ProviderRow>(@"
                SELECT
                    P.Id AS ProviderId,
                    P.LastName AS ProviderLastName,
                    P.FirstName AS ProviderFirstName,
                    P.Type AS ProviderType
                FROM Clinical.Providers AS P
                WHERE P.Type = {0}", (int)TargetProviderType).ToList();
        }

        protected override Task<IEnumerable<ProviderRow>?> ResolveRecord(ProviderAssignment record, IReadOnlyCollection<ProviderRow> cachedItems)
        {
            var lastName = GetLastNameFromRecord(record);
            if (string.IsNullOrEmpty(lastName))
            {
                return Task.FromResult<IEnumerable<ProviderRow>?>(null);
            }

            var matches = cachedItems.Where(p =>
                string.Equals(p.ProviderLastName, lastName, StringComparison.OrdinalIgnoreCase)).ToList();
            return Task.FromResult<IEnumerable<ProviderRow>?>(matches);
        }

        protected override void SetResolvedId(ProviderAssignment record, ProviderRow entity)
        {
            SetResolvedProviderIdOnRecord(record, entity.ProviderId);
        }
    }
}
