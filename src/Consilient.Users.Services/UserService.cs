using Consilient.Users.Contracts;
using Microsoft.AspNetCore.Identity;

namespace Consilient.Users.Services
{
    public class UserService(UserServiceConfiguration configuration, UserManager<IdentityUser> userManager, TokenGeneratorConfiguration tokenGeneratorConfiguration, Data.UsersDbContext dbContext) : IUserService
    {
        private readonly Data.UsersDbContext _dbContext = dbContext;
        private readonly TokenGenerator _tokenGenerator = new(tokenGeneratorConfiguration);
        public async Task<AuthenticateUserResult> AuthenticateUserAsync(AuthenticateUserRequest request)
        {
            static AuthenticateUserResult InvalidCredentials() => new(false, null, [ErrorMessages.InvalidCredentials]);

            // Validate credentials without creating a sign-in session
            var user = await GetUserByEmailAsync(request.Email).ConfigureAwait(false);
            if (user == null)
            {
                return InvalidCredentials();
            }

            // enforce allowed email domains
            if (!IsEmailDomainAllowed(user.Email))
            {
                // Avoid leaking whether account exists — treat as invalid credentials
                return InvalidCredentials();
            }

            var passwordValid = await userManager.CheckPasswordAsync(user, request.Password).ConfigureAwait(false);
            if (!passwordValid)
            {
                return InvalidCredentials();
            }

            // generate JWT using token generator
            var tokenString = _tokenGenerator.GenerateToken(user);

            return new AuthenticateUserResult(true, tokenString);
        }

        public async Task<LinkExternalLoginResult> LinkExternalLoginAsync(LinkExternalLoginRequest request)
        {
            // enforce allowed email domains for the request email
            if (!IsEmailDomainAllowed(request.Email))
            {
                return new LinkExternalLoginResult(false, [ErrorMessages.EmailDomainNotAllowed]);
            }

            // Check if the external login (provider + key) is already linked to any user
            var existingLoginUser = await userManager.FindByLoginAsync(request.Provider, request.ProviderKey).ConfigureAwait(false);

            // Find local user by email (may be null)
            var user = await GetUserByEmailAsync(request.Email).ConfigureAwait(false);

            if (existingLoginUser != null)
            {
                // If the login is linked to a different account, reject
                if (user == null || !string.Equals(existingLoginUser.Id, user.Id, StringComparison.Ordinal))
                {
                    return new LinkExternalLoginResult(false, [ErrorMessages.ExternalLoginAlreadyLinked]);
                }

                // Already linked to the same account: nothing to do
                return new LinkExternalLoginResult(true);
            }

            // Delegate transactional create+link to a helper for readability
            return await CreateAndLinkExternalLoginAsync(user, request).ConfigureAwait(false);
        }


        private static string[] MapIdentityErrors(IdentityResult result)
                    => result.Errors?.Select(e => e.Description).Where(d => !string.IsNullOrWhiteSpace(d)).ToArray()
                       ?? [ErrorMessages.UnexpectedError];

        // Helper: add external login and return (succeeded, errors)
        private async Task<(bool succeeded, string[]? errors)> AddExternalLoginAsync(IdentityUser user, UserLoginInfo login)
        {
            var result = await userManager.AddLoginAsync(user, login).ConfigureAwait(false);
            if (result.Succeeded)
            {
                return (true, null);
            }

            return (false, MapIdentityErrors(result).ToArray());
        }

        // Helper: encapsulate transactional create + add-login flow
        private async Task<LinkExternalLoginResult> CreateAndLinkExternalLoginAsync(IdentityUser? user, LinkExternalLoginRequest request)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                if (user == null && configuration.AutoProvisionUser)
                {
                    var (createdUser, createErrors) = await CreateUserAsync(request.Email).ConfigureAwait(false);
                    if (createErrors != null && createErrors.Length != 0)
                    {
                        await transaction.RollbackAsync().ConfigureAwait(false);
                        return new LinkExternalLoginResult(false, createErrors);
                    }

                    user = createdUser;
                }

                if (user == null)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    return new LinkExternalLoginResult(false, [ErrorMessages.UserNotFound]);
                }

                if (!IsEmailDomainAllowed(user.Email))
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    return new LinkExternalLoginResult(false, [ErrorMessages.EmailDomainNotAllowed]);
                }

                var (succeeded, addErrors) = await AddExternalLoginAsync(user, new UserLoginInfo(request.Provider, request.ProviderKey, request.ProviderDisplayName)).ConfigureAwait(false);
                if (!succeeded)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    return new LinkExternalLoginResult(false, addErrors);
                }

                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);

                return new LinkExternalLoginResult(true);
            }
            catch (Exception)
            {
                try { await transaction.RollbackAsync().ConfigureAwait(false); } catch { }
                return new LinkExternalLoginResult(false, [ErrorMessages.ExternalLoginFailed]);
            }
        }

        // Helper: create a user and return (user, errors)
        private async Task<(IdentityUser? user, string[]? errors)> CreateUserAsync(string email)
        {
            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user).ConfigureAwait(false);
            if (result.Succeeded)
            {
                return (user, null);
            }

            return (null, MapIdentityErrors(result).ToArray());
        }
        private Task<IdentityUser?> GetUserByEmailAsync(string email) => userManager.FindByEmailAsync(email);
        private bool IsEmailDomainAllowed(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            var at = email.LastIndexOf('@');
            if (at < 0 || at == email.Length - 1)
            {
                return false;
            }

            var domain = email[(at + 1)..].Trim().ToLowerInvariant();
            var allowed = configuration.AllowedEmailDomains;
            if (allowed == null || allowed.Length == 0)
            {
                return true; // no restriction configured
            }

            return allowed.Any(d => string.Equals(d?.Trim(), domain, StringComparison.OrdinalIgnoreCase));
        }
    }
}
