namespace Consilient.Users.Services;

public class TokenGeneratorOptions
{
    public const string SectionName = "Authentication:UserService:Jwt";

    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpiryMinutes { get; init; } = 60;
}
