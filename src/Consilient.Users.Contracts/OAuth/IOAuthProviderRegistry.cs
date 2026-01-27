namespace Consilient.Users.Contracts.OAuth;

/// <summary>
/// Registry for managing multiple OAuth providers.
/// </summary>
public interface IOAuthProviderRegistry
{
    /// <summary>
    /// Gets all supported OAuth provider names.
    /// </summary>
    IEnumerable<string> GetSupportedProviders();

    /// <summary>
    /// Gets the service for a specific OAuth provider.
    /// </summary>
    /// <param name="providerName">The provider name (e.g., "Microsoft", "Google").</param>
    /// <returns>The OAuth provider service.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the provider is not supported.</exception>
    IOAuthProviderService GetProvider(string providerName);

    /// <summary>
    /// Checks if a provider is supported.
    /// </summary>
    /// <param name="providerName">The provider name to check.</param>
    /// <returns>True if the provider is supported; otherwise, false.</returns>
    bool IsProviderSupported(string providerName);

    /// <summary>
    /// Tries to get the service for a specific OAuth provider.
    /// </summary>
    /// <param name="providerName">The provider name.</param>
    /// <param name="provider">The OAuth provider service if found; otherwise, null.</param>
    /// <returns>True if the provider was found; otherwise, false.</returns>
    bool TryGetProvider(string providerName, out IOAuthProviderService? provider);
}