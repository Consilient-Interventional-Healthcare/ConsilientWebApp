using Consilient.Users.Contracts.OAuth;
using Microsoft.Extensions.Logging;

namespace Consilient.Users.Services.OAuth
{
    /// <summary>
    /// Registry implementation that manages OAuth providers from configuration.
    /// Supports multiple providers and follows the Open/Closed Principle.
    /// </summary>
    public class OAuthProviderRegistry : IOAuthProviderRegistry
    {
        private readonly Dictionary<string, IOAuthProviderService> _providers;
        private readonly ILogger<OAuthProviderRegistry> _logger;

        public OAuthProviderRegistry(
            IEnumerable<IOAuthProviderService> providers,
            ILogger<OAuthProviderRegistry> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            ArgumentNullException.ThrowIfNull(providers);

            // Build provider dictionary with case-insensitive keys
            _providers = new Dictionary<string, IOAuthProviderService>(StringComparer.OrdinalIgnoreCase);

            foreach (var provider in providers)
            {
                var providerName = provider.GetProviderName();
                if (_providers.ContainsKey(providerName))
                {
                    _logger.LogWarning(
                        "Duplicate OAuth provider registration detected: {ProviderName}. Using first registration.",
                        providerName);
                    continue;
                }

                _providers[providerName] = provider;
                _logger.LogInformation("Registered OAuth provider: {ProviderName}", providerName);
            }

            if (_providers.Count == 0)
            {
                _logger.LogWarning("No OAuth providers registered in the system");
            }
        }

        public IEnumerable<string> GetSupportedProviders()
        {
            return _providers.Keys.ToList();
        }

        public IOAuthProviderService GetProvider(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                throw new ArgumentException("Provider name cannot be null or empty.", nameof(providerName));
            }

            if (!_providers.TryGetValue(providerName, out var provider))
            {
                throw new InvalidOperationException(
                    $"OAuth provider '{providerName}' is not supported. " +
                    $"Supported providers: {string.Join(", ", _providers.Keys)}");
            }

            return provider;
        }

        public bool IsProviderSupported(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                return false;
            }

            return _providers.ContainsKey(providerName);
        }

        public bool TryGetProvider(string providerName, out IOAuthProviderService? provider)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                provider = null;
                return false;
            }

            return _providers.TryGetValue(providerName, out provider);
        }
    }
}