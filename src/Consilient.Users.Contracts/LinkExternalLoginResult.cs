namespace Consilient.Users.Contracts
{
    public record LinkExternalLoginResult(bool Succeeded, IEnumerable<string>? Errors = null);
}
