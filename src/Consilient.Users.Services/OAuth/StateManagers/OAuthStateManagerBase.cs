using Consilient.Users.Contracts.OAuth;
using System.Security.Cryptography;
using System.Text;

namespace Consilient.Users.Services.OAuth.StateManagers
{
    /// <summary>
    /// Base class for OAuth state managers providing common validation logic.
    /// </summary>
    public abstract class OAuthStateManagerBase : IOAuthStateManager
    {
        public abstract Task<string> GenerateStateAsync(
            string returnUrl,
            string codeVerifier,
            string csrfToken,
            CancellationToken cancellationToken = default);

        public async Task<OAuthStateValidationResult> ValidateAndConsumeAsync(
            string state,
            string csrfToken,
            CancellationToken cancellationToken = default)
        {
            // Common input validation
            var validationError = ValidateInputs(state, csrfToken);
            if (validationError != null)
            {
                return validationError;
            }

            // Delegate to implementation-specific retrieval and removal
            var retrievalResult = await RetrieveAndRemoveStateAsync(state, cancellationToken).ConfigureAwait(false);
            if (!retrievalResult.IsSuccess)
            {
                return retrievalResult.ValidationResult;
            }

            // Common CSRF token validation
            if (string.IsNullOrEmpty(retrievalResult.StoredCsrfToken))
            {
                return new OAuthStateValidationResult(false, ErrorMessage: "Stored CSRF token is missing.");
            }
            var csrfValidationError = ValidateCsrfToken(retrievalResult.StoredCsrfToken, csrfToken);
            if (csrfValidationError != null)
            {
                return csrfValidationError;
            }

            return new OAuthStateValidationResult(
                true,
                retrievalResult.ReturnUrl,
                retrievalResult.CodeVerifier);
        }

        /// <summary>
        /// Validates the input parameters for state and CSRF token.
        /// </summary>
        /// <returns>Validation error result if validation fails, otherwise null.</returns>
        protected static OAuthStateValidationResult? ValidateInputs(string state, string csrfToken)
        {
            if (string.IsNullOrWhiteSpace(state))
            {
                return new OAuthStateValidationResult(false, ErrorMessage: "State token is required.");
            }

            if (string.IsNullOrWhiteSpace(csrfToken))
            {
                return new OAuthStateValidationResult(false, ErrorMessage: "CSRF token is required.");
            }

            return null;
        }

        /// <summary>
        /// Validates the CSRF token using constant-time comparison.
        /// </summary>
        /// <returns>Validation error result if validation fails, otherwise null.</returns>
        protected static OAuthStateValidationResult? ValidateCsrfToken(string storedCsrfToken, string providedCsrfToken)
        {
            var storedTokenBytes = Encoding.UTF8.GetBytes(storedCsrfToken);
            var providedTokenBytes = Encoding.UTF8.GetBytes(providedCsrfToken);

            if (storedTokenBytes.Length != providedTokenBytes.Length ||
                !CryptographicOperations.FixedTimeEquals(storedTokenBytes, providedTokenBytes))
            {
                return new OAuthStateValidationResult(false, ErrorMessage: "CSRF token mismatch.");
            }

            return null;
        }

        /// <summary>
        /// Retrieves and removes the state entry from storage.
        /// Implementations must handle expiration checking and removal atomically.
        /// </summary>
        /// <param name="state">The state token to retrieve and remove.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result containing the stored data if successful, or an error result.</returns>
        protected abstract Task<StateRetrievalResult> RetrieveAndRemoveStateAsync(
            string state,
            CancellationToken cancellationToken);

        /// <summary>
        /// Result of state retrieval operation.
        /// </summary>
        protected record StateRetrievalResult
        {
            public bool IsSuccess { get; init; }
            public string? ReturnUrl { get; init; }
            public string? CodeVerifier { get; init; }
            public string? StoredCsrfToken { get; init; }
            public OAuthStateValidationResult ValidationResult { get; init; } = null!;

            public static StateRetrievalResult Success(string returnUrl, string codeVerifier, string storedCsrfToken) =>
                new()
                {
                    IsSuccess = true,
                    ReturnUrl = returnUrl,
                    CodeVerifier = codeVerifier,
                    StoredCsrfToken = storedCsrfToken
                };

            public static StateRetrievalResult Failure(string errorMessage) =>
                new()
                {
                    IsSuccess = false,
                    ValidationResult = new OAuthStateValidationResult(false, ErrorMessage: errorMessage)
                };
        }
    }
}