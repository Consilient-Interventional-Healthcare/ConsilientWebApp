namespace Consilient.Users.Contracts;

public record ExternalAuthenticateRequest(string Provider, string Code, string CodeVerifier, string RedirectUri);