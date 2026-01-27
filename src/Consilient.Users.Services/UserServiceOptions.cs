using Consilient.Users.Contracts.OAuth;

namespace Consilient.Users.Services;

public class UserServiceOptions
{
    public const string SectionName = "Authentication:UserService";

    public bool AutoProvisionUser { get; init; } = false;

    /// <summary>
    /// List of email domains allowed to authenticate with the application.
    /// </summary>
    /// <remarks>
    /// Configuration format (JSON array of strings):
    /// <code>
    /// "AllowedEmailDomains": ["yourcompany.com", "subsidiary.com"]
    /// </code>
    ///
    /// Examples:
    /// - Single domain: ["yourcompany.com"]
    /// - Multiple domains: ["yourcompany.com", "subsidiary.com"]
    /// - Allow all domains: ["*"] (not recommended for production)
    ///
    /// At least one entry is required. Users whose email domain is not in the
    /// allowed list will be denied access even after successful OAuth authentication.
    /// </remarks>
    public string[] AllowedEmailDomains { get; init; } = null!;
    public TokenGeneratorOptions Jwt { get; init; } = null!;
    public OAuthProviderOptions? OAuth { get; init; }
}
