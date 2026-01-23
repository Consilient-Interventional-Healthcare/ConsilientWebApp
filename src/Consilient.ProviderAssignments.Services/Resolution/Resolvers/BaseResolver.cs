using Consilient.Data;
using Consilient.Data.Entities.Staging;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Microsoft.Extensions.Logging;

namespace Consilient.ProviderAssignments.Services.Resolution.Resolvers
{
    internal abstract class BaseResolver<TEntity, TResolver>(IResolutionCache cache, ConsilientDbContext dbContext, ILogger<TResolver> logger) : IResolver<TEntity> where TEntity : class
    {
        protected IResolutionCache Cache { get; } = cache;
        protected ConsilientDbContext DbContext { get; } = dbContext;
        protected ILogger<TResolver> Logger { get; } = logger;

        public async Task ResolveAsync(int facilityId, DateOnly date, List<ProviderAssignment> records)
        {
            if (records == null || records.Count == 0)
            {
                return;
            }
            var cachedItems = await LoadCache(facilityId, date);
            foreach (var record in records)
            {
                if (record.HasValidationErrors)
                {
                    continue;
                }
                var resolved = await ResolveRecord(record, cachedItems);
                if (resolved?.Count() == 1)
                {
                    SetResolvedId(record, resolved.First());
                }
                else if (resolved == null || !resolved.Any())
                {
                    Logger.LogDebug("{ResolverType}: No match found for record {RecordId} (MRN: {Mrn})",
                        GetType().Name, record.Id, record.Mrn);
                }
                else
                {
                    Logger.LogWarning("{ResolverType}: Multiple matches ({MatchCount}) found for record {RecordId} (MRN: {Mrn})",
                        GetType().Name, resolved.Count(), record.Id, record.Mrn);
                }
            }
        }

        protected abstract void SetResolvedId(ProviderAssignment record, TEntity entity);
        protected abstract Task<IEnumerable<TEntity>?> ResolveRecord(ProviderAssignment record, IReadOnlyCollection<TEntity> cachedItems);

        protected Task<IReadOnlyCollection<TEntity>> LoadCache(int facilityId, DateOnly date)
        {
            IReadOnlyCollection<TEntity> providers;
            if (!Cache.HasCache<TEntity>())
            {
                providers = Cache.FillCache(() => LoadEntities(facilityId, date));
            }
            else
            {
                providers = Cache.Get<TEntity>();
            }
            return Task.FromResult(providers);
        }

        protected abstract IReadOnlyCollection<TEntity> LoadEntities(int facilityId, DateOnly date);
    }
}
