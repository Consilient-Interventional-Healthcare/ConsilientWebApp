namespace Consilient.Api.Configuration
{
    public class AuthenticationSettings
    {
        public bool Enabled { get; init; }
        public JwtSettings Jwt { get; init; } = null!;
        public string[] AllowedEmailDomains { get; init; } = null!;
        public bool AutoProvisionUser { get; init; }
    }

    public class JwtSettings
    {
        public required string Issuer { get; init; }
        public required string Audience { get; init; }
        public required string SecretKey { get; init; }
        public required int ExpiryMinutes { get; init; }
    }
}
