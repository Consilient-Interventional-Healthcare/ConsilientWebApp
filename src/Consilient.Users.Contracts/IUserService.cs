namespace Consilient.Users.Contracts
{
    public interface IUserService
    {
        Task<AuthenticateUserResult> AuthenticateUserAsync(
            AuthenticateUserRequest request,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<ClaimDto>> GetClaimsAsync(
            string userName,
            CancellationToken cancellationToken = default);

        Task<LinkExternalLoginResult> LinkExternalLoginAsync(
            LinkExternalLoginRequest request,
            CancellationToken cancellationToken = default);

        Task<AuthenticateUserResult> AuthenticateExternalAsync(
            ExternalAuthenticateRequest request,
            CancellationToken cancellationToken = default);

        Task<string> BuildAuthorizationUrlAsync(
            string provider,
            string state,
            string codeChallenge,
            string redirectUri,
            CancellationToken cancellationToken = default);
    }
}