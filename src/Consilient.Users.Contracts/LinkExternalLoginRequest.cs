namespace Consilient.Users.Contracts
{
    public record LinkExternalLoginRequest(string Email, string Provider, string ProviderKey, string? ProviderDisplayName = null);
}
