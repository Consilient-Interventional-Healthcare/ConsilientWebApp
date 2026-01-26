using Consilient.Users.Services;

namespace Consilient.BackgroundHost.Configuration
{
    internal class AuthenticationSettings
    {
        public const string SectionName = "Authentication";

        public bool DashboardAuthEnabled { get; init; } = true;
        public UserServiceSettings? UserService { get; init; }
    }

    internal class UserServiceSettings
    {
        /// <summary>
        /// JWT configuration shared with Consilient.Users.Services.
        /// </summary>
        public TokenGeneratorOptions? Jwt { get; init; }
    }
}
