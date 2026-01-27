using Consilient.Users.Services;

namespace Consilient.Api.Configuration;

public class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public int CookieExpiryMinutes { get; init; }
    public bool Enabled { get; init; }
    public PasswordPolicyOptions PasswordPolicy { get; init; } = null!;
    public UserServiceOptions UserService { get; init; } = null!;
}
