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


}
