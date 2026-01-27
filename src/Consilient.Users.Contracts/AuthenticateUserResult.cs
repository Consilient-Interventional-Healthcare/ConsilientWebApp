namespace Consilient.Users.Contracts;

public record AuthenticateUserResult(bool Succeeded, string? Token, CurrentUserDto? User = null, IEnumerable<string>? Errors = null);
