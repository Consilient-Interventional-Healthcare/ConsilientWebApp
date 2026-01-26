using Consilient.Users.Contracts.OAuth;

namespace Consilient.BackgroundHost.Configuration
{
    internal class AuthenticationSettings
    {
        public const string SectionName = "Authentication";

        public UserServiceSettings? UserService { get; init; }
    }

    internal class UserServiceSettings
    {
        /// <summary>
        /// OAuth configuration for Azure Entra authentication.
        /// </summary>
        public OAuthProviderOptions? OAuth { get; init; }
    }
}
