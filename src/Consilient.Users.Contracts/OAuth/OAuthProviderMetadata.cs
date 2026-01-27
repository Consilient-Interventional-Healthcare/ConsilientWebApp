namespace Consilient.Users.Contracts.OAuth;

/// <summary>
/// Metadata about an OAuth provider for client consumption.
/// </summary>
public record OAuthProviderMetadata(
    string ProviderName,
    string DisplayName,
    string LoginUrl,
    string CallbackUrl,
    bool IsEnabled);