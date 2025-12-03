using System.Security.Claims;

namespace Consilient.Users.Contracts
{
    public record AuthenticateUserRequest(string UserName, string Password);
    public record ClaimDto(string Type, string Value);
    public record AuthenticateUserResult(bool Succeeded, string? Token = null, IEnumerable<ClaimDto>? Claims = null, IEnumerable<string>? Errors = null);

    public record LinkExternalLoginRequest(string Email, string Provider, string ProviderKey, string? ProviderDisplayName = null);
    public record LinkExternalLoginResult(bool Succeeded, IEnumerable<string>? Errors = null);

    public interface IUserService
    {
        Task<AuthenticateUserResult> AuthenticateUserAsync(AuthenticateUserRequest request);
        Task<IEnumerable<ClaimDto>> GetClaimsAsync(string userName);
        Task<LinkExternalLoginResult> LinkExternalLoginAsync(LinkExternalLoginRequest request);
    }
}
