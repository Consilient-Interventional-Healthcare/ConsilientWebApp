namespace Consilient.BackgroundHost.Configuration
{
    internal class ApplicationSettings
    {
        public AuthenticationSettings? Authentication { get; init; }
    }

    internal class AuthenticationSettings
    {
        public bool DashboardAuthEnabled { get; init; } = true;
        public UserServiceSettings? UserService { get; init; }
    }

    internal class UserServiceSettings
    {
        public JwtSettings? Jwt { get; init; }
    }

    internal class JwtSettings
    {
        public string Secret { get; init; } = string.Empty;
        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
    }
}
