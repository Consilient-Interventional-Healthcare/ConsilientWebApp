namespace Consilient.Users.Services;

public class PasswordPolicyOptions
{
    public bool RequireDigit { get; init; } = true;
    public int RequiredLength { get; init; } = 8;
    public bool RequireNonAlphanumeric { get; init; } = false;
    public bool RequireUppercase { get; init; } = true;
    public bool RequireLowercase { get; init; } = true;
    public int RequiredUniqueChars { get; init; } = 1;
}