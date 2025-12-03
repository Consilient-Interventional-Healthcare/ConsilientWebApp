using Consilient.Users.Services;

namespace Consilient.Api.Configuration
{
    public class AuthenticationSettings
    {
        public bool Enabled { get; init; }
        public TokenGeneratorConfiguration Jwt { get; init; } = null!;
        public string[] AllowedEmailDomains { get; init; } = null!;
        public bool AutoProvisionUser { get; init; }
    }
}
