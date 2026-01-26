namespace Consilient.Users.Contracts.OAuth
{
    public class OAuthProviderOptions
    {
        public const string SectionName = "Authentication:UserService:OAuth";

        public bool Enabled { get; init; } = false;
        public string? Authority { get; init; }
        public string ProviderName { get; init; } = string.Empty;
        public string? ClientId { get; init; }
        public string? ClientSecret { get; init; }
        public string? TenantId { get; init; }
        public IEnumerable<string>? Scopes { get; init; }
    }
}
