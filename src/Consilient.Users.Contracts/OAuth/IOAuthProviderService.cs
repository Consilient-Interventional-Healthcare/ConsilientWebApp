namespace Consilient.Users.Contracts.OAuth;

/// <summary>
/// Service interface for OAuth provider operations.
/// </summary>
public interface IOAuthProviderService
{
    /// <summary>
    /// Gets the name of this OAuth provider (e.g., "Microsoft", "Google").
    /// </summary>
    string GetProviderName();

    /// <summary>
    /// Builds the authorization URL for initiating OAuth flow.
    /// </summary>
    Task<string> BuildAuthorizationUrlAsync(
        string state,
        string codeChallenge,
        string redirectUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an authorization code and returns user information.
    /// </summary>
    Task<AuthorizationCodeValidationResult> ValidateAuthorizationCodeAsync(
        string code,
        string codeVerifier,
        string redirectUri,
        CancellationToken cancellationToken = default);
}