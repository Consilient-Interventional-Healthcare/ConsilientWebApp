namespace Consilient.Users.Contracts
{
    public interface IUserService
    {
        Task<AuthenticateUserResult> AuthenticateUserAsync(AuthenticateUserRequest request);

        Task<IEnumerable<ClaimDto>> GetClaimsAsync(string userName);

        Task<LinkExternalLoginResult> LinkExternalLoginAsync(LinkExternalLoginRequest request);

        Task<AuthenticateUserResult> AuthenticateExternalAsync(ExternalAuthenticateRequest request);
    }
}