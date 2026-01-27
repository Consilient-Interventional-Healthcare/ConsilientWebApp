using Consilient.ProviderAssignments.Contracts.Resolution;
using System.Collections.Concurrent;

namespace Consilient.ProviderAssignments.Services.Resolution;

internal class ResolutionCache : IResolutionCache
{
    private readonly ConcurrentDictionary<string, object> _cache = new();

    public IReadOnlyCollection<T> FillCache<T>(Func<IReadOnlyCollection<T>> loader)
    {
        var key = GetKey<T>();
        return (IReadOnlyCollection<T>)_cache.GetOrAdd(key, _ => loader());
    }

    public IReadOnlyCollection<T> Get<T>()
    {
        var key = GetKey<T>();
        if (_cache.TryGetValue(key, out var value))
        {
            return (IReadOnlyCollection<T>)value;
        }

        return [];
    }

    public bool HasCache<T>()
    {
        var key = GetKey<T>();
        return _cache.ContainsKey(key);
    }

    private static string GetKey<T>() => typeof(T).FullName ?? string.Empty;
}
