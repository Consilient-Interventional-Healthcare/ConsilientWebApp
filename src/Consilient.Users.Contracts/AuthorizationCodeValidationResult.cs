namespace Consilient.Users.Contracts
{
    public class AuthorizationCodeValidationResult
    {
        public bool Success { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string ProviderKey { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Error { get; set; }
    }
}