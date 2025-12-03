namespace Consilient.Users.Services
{
    public class UserServiceConfiguration
    {
        public bool AutoProvisionUser { get; set; } = false;
        public string[] AllowedEmailDomains { get; set; } = null!;
        public MicrosoftProviderSettings? MicrosoftProviderSettings { get; set; }
    }

    public sealed class MicrosoftProviderSettings
    {
        public bool Enabled { get; init; } = false;
        public string? ClientId { get; init; }
        public string? ClientSecret { get; init; } // keep in user secrets or KeyVault in production
        public string? TenantId { get; init; } = "common";
        public string? RedirectUri { get; init; }
        public IEnumerable<string>? Scopes { get; init; } = new List<string> { "User.Read" };
    }
}
