using Consilient.Users.Services;

namespace Consilient.BackgroundHost.Configuration
{
    internal class UserServiceSettings
    {
        /// <summary>
        /// JWT configuration shared with Consilient.Users.Services.
        /// </summary>
        public TokenGeneratorOptions? Jwt { get; init; }
    }
}
