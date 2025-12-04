using Consilient.Data.Entities.Identity;
using Consilient.Users.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace Consilient.Users.Services
{
    public class UserService(
        UserServiceConfiguration configuration,
        UserManager<User> userManager,
        TokenGeneratorConfiguration tokenGeneratorConfiguration,
        Data.UsersDbContext dbContext
    ) : IUserService
    {
        private readonly Data.UsersDbContext _dbContext = dbContext;
        private readonly TokenGenerator _tokenGenerator = new(tokenGeneratorConfiguration);

        public async Task<AuthenticateUserResult> AuthenticateExternalAsync(ExternalAuthenticateRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            var provider = "Microsoft";

            if (string.IsNullOrWhiteSpace(request.Provider) || !string.Equals(request.Provider, provider, StringComparison.OrdinalIgnoreCase))
            {
                return new AuthenticateUserResult(false, null, null, ["Unsupported external provider."]);
            }

            if (string.IsNullOrWhiteSpace(request.IdToken))
            {
                return new AuthenticateUserResult(false, null, null, ["id_token is required."]);
            }

            var (idToken, _, account, userEmail, userName) = await ValidateCode(request.IdToken);

            //catch (SecurityTokenExpiredException)
            //{
            //    return new AuthenticateUserResult(false, null, null, ["Token expired."]);
            //}
            //catch (SecurityTokenException)
            //{
            //    return new AuthenticateUserResult(false, null, null, ["Invalid token."]);
            //}
            //catch (Exception)
            //{
            //    return new AuthenticateUserResult(false, null, null, ["Failed to validate token."]);
            //}

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(userEmail))
            {
                return new AuthenticateUserResult(false, null, null, ["Required claims not present in token."]);
            }

            // enforce allowed email domains
            if (!EmailDomainHelper.IsEmailDomainAllowed(userEmail, configuration.AllowedEmailDomains))
            {
                return new AuthenticateUserResult(false, null, null, [ErrorMessages.EmailDomainNotAllowed]);
            }

            // Try find existing user by external login (provider + key)
            var existingLoginUser = await userManager.FindByLoginAsync(provider, idToken).ConfigureAwait(false);
            if (existingLoginUser != null)
            {
                var token = _tokenGenerator.GenerateToken(existingLoginUser);
                var claimDtos = await GetClaimsAsync(existingLoginUser).ConfigureAwait(false);
                return new AuthenticateUserResult(true, token, claimDtos);
            }

            // Find local user by email
            var user = await userManager.FindByEmailAsync(userEmail).ConfigureAwait(false);

            // If user exists, attach external login (if allowed) and sign in
            if (user != null)
            {
                // If linking fails, return errors to caller
                var (succeeded, errors) = await AddExternalLoginAsync(user, new UserLoginInfo(provider, idToken, provider)).ConfigureAwait(false);
                if (!succeeded)
                {
                    return new AuthenticateUserResult(false, null, null, errors);
                }

                // Persist the new login
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);

                var token = _tokenGenerator.GenerateToken(user);
                var claimDtos = await GetClaimsAsync(user).ConfigureAwait(false);
                return new AuthenticateUserResult(true, token, claimDtos);
            }

            // User does not exist locally
            if (configuration.AutoProvisionUser)
            {
                // Create user, add external login and sign in (transactional)
                var linkRequest = new LinkExternalLoginRequest(userEmail, provider, account.HomeAccountId.Identifier, provider);
                var result = await CreateAndLinkExternalLoginAsync(null, linkRequest).ConfigureAwait(false);
                if (!result.Succeeded)
                {
                    return new AuthenticateUserResult(false, null, null, result.Errors);
                }

                // retrieve created user (by email) and issue token
                var createdUser = await userManager.FindByEmailAsync(userEmail).ConfigureAwait(false);
                if (createdUser == null)
                {
                    return new AuthenticateUserResult(false, null, null, [ErrorMessages.UnexpectedError]);
                }

                var token = _tokenGenerator.GenerateToken(createdUser);
                var claimDtos = await GetClaimsAsync(createdUser).ConfigureAwait(false);
                return new AuthenticateUserResult(true, token, claimDtos);
            }

            // Not auto-provisioning: return informative error so frontend can show registration flow
            return new AuthenticateUserResult(false, null, null, [ErrorMessages.UserNotFound]);
        }

        private async Task<(string idToken, string accessToken, IAccount account, string userEmail, string userName)> ValidateCode(string code)
        {
            var conf = configuration.MicrosoftProviderSettings!;
            var app = ConfidentialClientApplicationBuilder
              .Create(conf.ClientId)
              .WithClientSecret(conf.ClientSecret)
              .WithAuthority($"https://login.microsoftonline.com/{conf.TenantId}")
              //.WithRedirectUri(redirectUri)
              .Build();

            // Exchange authorization code for tokens
            var result = await app.AcquireTokenByAuthorizationCode(conf.Scopes, code).ExecuteAsync();

            // Token is now validated! Extract user information
            var idToken = result.IdToken;
            var accessToken = result.AccessToken;
            var account = result.Account;

            // Get user claims from the ID token
            var userEmail = account.Username;
            var userName = account.HomeAccountId.ObjectId;
            return (idToken, accessToken, account, userEmail, userName);
        }

        public async Task<AuthenticateUserResult> AuthenticateUserAsync(AuthenticateUserRequest request)
        {
            static AuthenticateUserResult InvalidCredentials() => new(false, null, null, [ErrorMessages.InvalidCredentials]);

            // Validate credentials without creating a sign-in session
            var user = await userManager.FindByNameAsync(request.UserName).ConfigureAwait(false);
            if (user == null)
            {
                return InvalidCredentials();
            }

            var passwordValid = await userManager.CheckPasswordAsync(user, request.Password).ConfigureAwait(false);
            if (!passwordValid)
            {
                return InvalidCredentials();
            }
            // generate JWT using token generator
            var tokenString = _tokenGenerator.GenerateToken(user);
            var claimDtos = await GetClaimsAsync(user).ConfigureAwait(false);
            return new AuthenticateUserResult(true, tokenString, claimDtos);
        }

        public async Task<IEnumerable<ClaimDto>> GetClaimsAsync(string userName)
        {
            var user = await userManager.FindByNameAsync(userName).ConfigureAwait(false) ?? throw new ArgumentException(ErrorMessages.UserNotFound, nameof(userName));
            return [
                new (ClaimTypes.NameIdentifier, user.Id.ToString()),
                new (ClaimTypes.Name, user.UserName ?? string.Empty),
                new (ClaimTypes.Email, user.Email ?? string.Empty)
            ];
        }

        public async Task<LinkExternalLoginResult> LinkExternalLoginAsync(LinkExternalLoginRequest request)
        {
            // enforce allowed email domains for the request email
            if (!EmailDomainHelper.IsEmailDomainAllowed(request.Email, configuration.AllowedEmailDomains))
            {
                return new LinkExternalLoginResult(false, [ErrorMessages.EmailDomainNotAllowed]);
            }

            // Check if the external login (provider + key) is already linked to any user
            var existingLoginUser = await userManager.FindByLoginAsync(request.Provider, request.ProviderKey).ConfigureAwait(false);

            // Find local user by email (may be null)
            var user = await userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);

            if (existingLoginUser != null)
            {
                // If the login is linked to a different account, reject
                if (user == null || existingLoginUser.Id != user.Id)
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
        private async Task<(bool succeeded, string[]? errors)> AddExternalLoginAsync(User user, UserLoginInfo login)
        {
            var result = await userManager.AddLoginAsync(user, login).ConfigureAwait(false);
            if (result.Succeeded)
            {
                return (true, null);
            }

            return (false, MapIdentityErrors(result).ToArray());
        }

        // Helper: encapsulate transactional create + add-login flow
        private async Task<LinkExternalLoginResult> CreateAndLinkExternalLoginAsync(User? user, LinkExternalLoginRequest request)
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

                if (!EmailDomainHelper.IsEmailDomainAllowed(user.Email, configuration.AllowedEmailDomains))
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
        private async Task<(User? user, string[]? errors)> CreateUserAsync(string email)
        {
            var user = new User
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

        private async Task<IEnumerable<ClaimDto>> GetClaimsAsync(User user)
        {
            // build claims to return to caller
            var claims = await userManager.GetClaimsAsync(user).ConfigureAwait(false);
            var claimDtos = claims.Select(c => new ClaimDto(c.Type, c.Value)).ToArray();
            return claimDtos;
        }
    }
}