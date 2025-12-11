using System.Collections.Concurrent;

namespace Consilient.Users.Services.OAuth.StateManagers
{
    /// <summary>
    /// In-memory OAuth state manager with CSRF protection.
    /// Suitable for development and single-server deployments.
    /// For production multi-server deployments, use DistributedOAuthStateManager.
    /// </summary>
    internal class InMemoryOAuthStateManager : OAuthStateManagerBase, IDisposable
    {
        private readonly ConcurrentDictionary<string, StateEntry> _store = new();
        private readonly Timer _cleanupTimer;

        private record StateEntry(string ReturnUrl, string CodeVerifier, string CsrfToken, DateTime ExpiresAt);

        public InMemoryOAuthStateManager()
        {
            _cleanupTimer = new Timer(
                _ => CleanupExpired(),
                null,
                TimeSpan.FromMinutes(OAuthSecurityConstants.CleanupIntervalMinutes),
                TimeSpan.FromMinutes(OAuthSecurityConstants.CleanupIntervalMinutes));
        }

        public override Task<string> GenerateStateAsync(
            string returnUrl,
            string codeVerifier,
            string csrfToken,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(returnUrl);
            ArgumentException.ThrowIfNullOrWhiteSpace(codeVerifier);
            ArgumentException.ThrowIfNullOrWhiteSpace(csrfToken);

            cancellationToken.ThrowIfCancellationRequested();

            var state = CryptographicTokenGenerator.Generate(OAuthSecurityConstants.TokenSizeBytes);
            _store[state] = new StateEntry(
                returnUrl,
                codeVerifier,
                csrfToken,
                DateTime.UtcNow.AddMinutes(OAuthSecurityConstants.StateExpirationMinutes));

            return Task.FromResult(state);
        }

        protected override Task<StateRetrievalResult> RetrieveAndRemoveStateAsync(
            string state,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_store.TryRemove(state, out var entry))
            {
                return Task.FromResult(
                    StateRetrievalResult.Failure("Invalid or expired state token."));
            }

            if (entry.ExpiresAt <= DateTime.UtcNow)
            {
                return Task.FromResult(
                    StateRetrievalResult.Failure("State token has expired."));
            }

            return Task.FromResult(
                StateRetrievalResult.Success(entry.ReturnUrl, entry.CodeVerifier, entry.CsrfToken));
        }

        private void CleanupExpired()
        {
            var now = DateTime.UtcNow;
            var expired = _store.Where(kv => kv.Value.ExpiresAt <= now)
                               .Select(kv => kv.Key)
                               .ToList();

            foreach (var key in expired)
            {
                _store.TryRemove(key, out _);
            }
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}