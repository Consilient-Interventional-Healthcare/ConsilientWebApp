using Consilient.Users.Services;

namespace Consilient.Api.Configuration
{
    public class AuthenticationSettings
    {
        public bool Enabled { get; init; }
        public TokenGeneratorConfiguration Jwt { get; init; } = null!;
        public ExternalProvidersSettings External { get; init; } = new ExternalProvidersSettings();
        public string[] AllowedEmailDomains { get; init; } = null!;
        public bool AutoProvisionUser { get; init; }
    }
    public sealed class ExternalProvidersSettings
    {
        public MicrosoftProviderSettings Microsoft { get; init; } = new MicrosoftProviderSettings();
    }

    public sealed class MicrosoftProviderSettings
    {
        public bool Enabled { get; init; } = false;
        public string? ClientId { get; init; }
        public string? ClientSecret { get; init; } // keep in user secrets or KeyVault in production
        public string? TenantId { get; init; } = "common";
        public string? RedirectUri { get; init; }
    }
}
