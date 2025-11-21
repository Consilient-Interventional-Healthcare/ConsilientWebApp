namespace Consilient.Users.Services
{
    public class TokenGeneratorConfiguration
    {
        public string? Secret { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public int ExpiryMinutes { get; set; } = 60;
    }
}
