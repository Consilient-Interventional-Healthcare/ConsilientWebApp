using Consilient.Users.Services;

namespace Consilient.Api.Configuration
{
    public class AuthenticationSettings
    {
        public int CookieExpiryMinutes { get; init; }
        public bool Enabled { get; init; }
        public PasswordPolicyOptions PasswordPolicy { get; init; } = null!;
        public UserServiceConfiguration UserService { get; init; } = null!;
    }
}
