using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Consilient.Users.Services.OAuth.StateManagers;

/// <summary>
/// Distributed cache-based OAuth state manager with CSRF protection.
/// Suitable for production use with Redis or SQL Server cache.
/// </summary>
public class DistributedOAuthStateManager(IDistributedCache cache) : OAuthStateManagerBase
{
    private readonly IDistributedCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private const string CacheKeyPrefix = "oauth_state:";

    private record StateEntry(string ReturnUrl, string CodeVerifier, string CsrfToken);

    public override async Task<string> GenerateStateAsync(
        string returnUrl,
        string codeVerifier,
        string csrfToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(returnUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(codeVerifier);
        ArgumentException.ThrowIfNullOrWhiteSpace(csrfToken);

        var state = CryptographicTokenGenerator.Generate(OAuthSecurityConstants.TokenSizeBytes);
        var entry = new StateEntry(returnUrl, codeVerifier, csrfToken);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(OAuthSecurityConstants.StateExpirationMinutes)
        };

        var json = JsonSerializer.Serialize(entry);
        await _cache.SetStringAsync(
            $"{CacheKeyPrefix}{state}",
            json,
            options,
            cancellationToken)
            .ConfigureAwait(false);

        return state;
    }

    protected override async Task<StateRetrievalResult> RetrieveAndRemoveStateAsync(
        string state,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{CacheKeyPrefix}{state}";
        var json = await _cache.GetStringAsync(cacheKey, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(json))
        {
            return StateRetrievalResult.Failure("Invalid or expired state token.");
        }

        // Remove the state immediately to prevent replay attacks
        await _cache.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);

        StateEntry? entry;
        try
        {
            entry = JsonSerializer.Deserialize<StateEntry>(json);
        }
        catch (JsonException)
        {
            return StateRetrievalResult.Failure("Invalid state token format.");
        }

        if (entry == null)
        {
            return StateRetrievalResult.Failure("Invalid state token data.");
        }

        return StateRetrievalResult.Success(entry.ReturnUrl, entry.CodeVerifier, entry.CsrfToken);
    }
}