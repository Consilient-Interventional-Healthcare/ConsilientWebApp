namespace Consilient.Users.Contracts
{
    public record AuthenticateUserResult(bool Succeeded, string? Token, IEnumerable<ClaimDto>? Claims = null, IEnumerable<string>? Errors = null);
}
