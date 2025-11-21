namespace Consilient.Users.Contracts
{
    public record CreateUserRequest(string Email, string Password);
    public record CreateUserResult(bool Succeeded, IEnumerable<string>? Errors = null);

    public record AuthenticateUserRequest(string Email, string Password);
    public record AuthenticateUserResult(bool Succeeded, string? Token = null, IEnumerable<string>? Errors = null);

    public record LinkExternalLoginRequest(string Email, string Provider, string ProviderKey, string? ProviderDisplayName = null);
    public record LinkExternalLoginResult(bool Succeeded, IEnumerable<string>? Errors = null);

    public interface IUserService
    {
        Task<CreateUserResult> CreateUserAsync(CreateUserRequest request);
        Task<AuthenticateUserResult> AuthenticateUserAsync(AuthenticateUserRequest request);
        Task<LinkExternalLoginResult> LinkExternalLoginAsync(LinkExternalLoginRequest request);
    }
}
