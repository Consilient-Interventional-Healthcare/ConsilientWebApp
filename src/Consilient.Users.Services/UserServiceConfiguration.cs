using Consilient.Users.Contracts.OAuth;

namespace Consilient.Users.Services
{
    public class UserServiceConfiguration
    {
        public bool AutoProvisionUser { get; set; } = false;
        public string[] AllowedEmailDomains { get; set; } = null!;
        public TokenGeneratorConfiguration Jwt { get; set; } = null!;
        public OAuthProviderServiceConfiguration? OAuth { get; set; }
    }
}
