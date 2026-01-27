namespace Consilient.Users.Contracts.OAuth;

/// <summary>
/// Manages OAuth state tokens with CSRF protection.
/// </summary>
public interface IOAuthStateManager
{
    /// <summary>
    /// Generates a state token and associates it with the return URL, code verifier, and CSRF token.
    /// </summary>
    /// <param name="returnUrl">The URL to redirect to after authentication.</param>
    /// <param name="codeVerifier">The PKCE code verifier.</param>
    /// <param name="csrfToken">The CSRF token for additional security.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated state token.</returns>
    Task<string> GenerateStateAsync(
        string returnUrl,
        string codeVerifier,
        string csrfToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and consumes a state token, returning associated data.
    /// </summary>
    /// <param name="state">The state token to validate.</param>
    /// <param name="csrfToken">The CSRF token to validate against.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result containing return URL and code verifier if successful.</returns>
    Task<OAuthStateValidationResult> ValidateAndConsumeAsync(
        string state,
        string csrfToken,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of OAuth state validation.
/// </summary>
public record OAuthStateValidationResult(
    bool IsValid,
    string? ReturnUrl = null,
    string? CodeVerifier = null,
    string? ErrorMessage = null);