namespace Consilient.Users.Contracts
{
    public record ExternalAuthenticateRequest(string Provider, string IdToken);
}