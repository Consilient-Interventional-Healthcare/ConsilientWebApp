using Consilient.Data;
using Consilient.Data.Entities.Staging;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Consilient.ProviderAssignments.Contracts.Validation;
using Microsoft.Extensions.Logging;

namespace Consilient.ProviderAssignments.Services.Resolution.Resolvers;

internal abstract class BaseResolver<TEntity, TResolver>(IResolutionCache cache, ConsilientDbContext dbContext, ILogger<TResolver> logger) : IResolver<TEntity> where TEntity : class
{
    protected IResolutionCache Cache { get; } = cache;
    protected ConsilientDbContext DbContext { get; } = dbContext;
    protected ILogger<TResolver> Logger { get; } = logger;

    public async Task ResolveAsync(int facilityId, DateOnly date, List<IRowValidationContext> contexts)
    {
        if (contexts == null || contexts.Count == 0)
        {
            return;
        }
        var cachedItems = await LoadCache(facilityId, date);
        foreach (var ctx in contexts)
        {
            if (ctx.HasErrors)
            {
                continue;
            }
            var resolved = await ResolveRecord(ctx, cachedItems);
            if (resolved?.Count() == 1)
            {
                SetResolvedId(ctx, resolved.First());
            }
            else if (resolved == null || !resolved.Any())
            {
                Logger.LogDebug("{ResolverType}: No match found for record {RecordId} (MRN: {Mrn})",
                    GetType().Name, ctx.Row.Id, ctx.Row.Mrn);
            }
            else
            {
                Logger.LogWarning("{ResolverType}: Multiple matches ({MatchCount}) found for record {RecordId} (MRN: {Mrn})",
                    GetType().Name, resolved.Count(), ctx.Row.Id, ctx.Row.Mrn);
            }
        }
    }

    protected abstract void SetResolvedId(IRowValidationContext ctx, TEntity entity);
    protected abstract Task<IEnumerable<TEntity>?> ResolveRecord(IRowValidationContext ctx, IReadOnlyCollection<TEntity> cachedItems);

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
