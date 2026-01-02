using Consilient.Data.Entities.Identity;
using Consilient.Users.Contracts;
using Consilient.Users.Contracts.OAuth;
using Consilient.Users.Services.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Consilient.Users.Services
{
    public class UserService(
        IOptions<UserServiceConfiguration> configuration,
        UserManager<User> userManager,
        IOAuthProviderRegistry oauthProviderRegistry,
        ITokenGenerator tokenGenerator,
        ILogger<UserService> logger) : IUserService
    {
        private readonly UserServiceConfiguration _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
        private readonly ILogger<UserService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IOAuthProviderRegistry _oauthProviderRegistry = oauthProviderRegistry ?? throw new ArgumentNullException(nameof(oauthProviderRegistry));
        private readonly ITokenGenerator _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        private readonly UserManager<User> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

        public async Task<AuthenticateUserResult> AuthenticateExternalAsync(
            ExternalAuthenticateRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.Provider))
            {
                _logger.LogWarning("Provider missing in external authentication request");
                return IdentityHelper.CreateFailureResult(["Provider is required."]);
            }

            if (!_oauthProviderRegistry.TryGetProvider(request.Provider, out var oauthService))
            {
                _logger.LogWarning("Unsupported external provider requested: {Provider}", request.Provider);
                return IdentityHelper.CreateFailureResult(["Unsupported external provider."]);
            }

            if (string.IsNullOrWhiteSpace(request.Code))
            {
                _logger.LogWarning("Authorization code missing in external authentication request");
                return IdentityHelper.CreateFailureResult(["Authorization code is required."]);
            }

            var result = await oauthService!
                .ValidateAuthorizationCodeAsync(request.Code, request.CodeVerifier, request.RedirectUri, cancellationToken)
                .ConfigureAwait(false);

            if (!result.Succeeded)
            {
                return IdentityHelper.CreateFailureResult([result.Error!]);
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(result.UserName) || string.IsNullOrWhiteSpace(result.UserEmail))
            {
                _logger.LogWarning("Required claims not present in token. Email: {Email}, UserId: {UserId}",
                    result.UserEmail, result.UserName);
                return IdentityHelper.CreateFailureResult(["Required claims not present in token."]);
            }

            if (!EmailDomainHelper.IsEmailDomainAllowed(result.UserEmail, _configuration.AllowedEmailDomains))
            {
                _logger.LogWarning("External authentication blocked: email domain not allowed. Email: {Email}",
                    result.UserEmail);
                return IdentityHelper.CreateFailureResult([ErrorMessages.EmailDomainNotAllowed]);
            }

            var existingLoginUser = await _userManager
                .FindByLoginAsync(result.ProviderName, result.ProviderKey)
                .ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            if (existingLoginUser != null)
            {
                _logger.LogInformation("Existing external login found. UserId: {UserId}, Email: {Email}",
                    existingLoginUser.Id, existingLoginUser.Email);
                return await CreateSuccessResultAsync(existingLoginUser);
            }

            return await CreateOrLinkUserWithExternalLoginAsync(
                result.UserEmail,
                result.ProviderName,
                result.ProviderKey);
        }

        public async Task<string> BuildAuthorizationUrlAsync(
            string provider,
            string state,
            string codeChallenge,
            string redirectUri,
            CancellationToken cancellationToken = default)
        {
            var oauthService = _oauthProviderRegistry.GetProvider(provider);
            return await oauthService.BuildAuthorizationUrlAsync(
                state,
                codeChallenge,
                redirectUri,
                cancellationToken);
        }

        public async Task<AuthenticateUserResult> AuthenticateUserAsync(
            AuthenticateUserRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Attempting username/password authentication. Username: {Username}", request.UserName);

            cancellationToken.ThrowIfCancellationRequested();

            var user = await _userManager.FindByNameAsync(request.UserName).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                _logger.LogWarning("Authentication failed: user not found. Username: {Username}", request.UserName);
                return IdentityHelper.CreateInvalidCredentialsResult();
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password).ConfigureAwait(false);
            if (!passwordValid)
            {
                _logger.LogWarning("Authentication failed: invalid password. UserId: {UserId}", user.Id);
                return IdentityHelper.CreateInvalidCredentialsResult();
            }

            _logger.LogInformation("User authenticated successfully. UserId: {UserId}, Username: {Username}",
                user.Id, user.UserName);
            return await CreateSuccessResultAsync(user);
        }

        public async Task<IEnumerable<ClaimDto>> GetClaimsAsync(
            string userName,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByNameAsync(userName).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"User not found: {userName}");

            var claims = await _userManager.GetClaimsAsync(user).ConfigureAwait(false);
            return ClaimHelper.MapToDto(ClaimHelper.MergeClaims(user, claims));
        }

        public async Task<LinkExternalLoginResult> LinkExternalLoginAsync(
            LinkExternalLoginRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!EmailDomainHelper.IsEmailDomainAllowed(request.Email, _configuration.AllowedEmailDomains))
            {
                _logger.LogWarning("Link external login blocked: email domain not allowed. Email: {Email}",
                    request.Email);
                return new LinkExternalLoginResult(false, [ErrorMessages.EmailDomainNotAllowed]);
            }

            var existingLoginUser = await _userManager
                .FindByLoginAsync(request.Provider, request.ProviderKey)
                .ConfigureAwait(false);
            var user = await _userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);

            if (existingLoginUser != null)
            {
                if (user == null || existingLoginUser.Id != user.Id)
                {
                    _logger.LogWarning(
                        "External login already linked to different account. Email: {Email}, Provider: {Provider}",
                        request.Email, request.Provider);
                    return new LinkExternalLoginResult(false, [ErrorMessages.ExternalLoginAlreadyLinked]);
                }

                _logger.LogInformation("External login already linked to same account. UserId: {UserId}", user.Id);
                return new LinkExternalLoginResult(true);
            }

            return await CreateAndLinkExternalLoginAsync(user, request).ConfigureAwait(false);
        }

        private async Task<LinkExternalLoginResult> CreateAndLinkExternalLoginAsync(
            User? user,
            LinkExternalLoginRequest request)
        {
            if (user == null && !_configuration.AutoProvisionUser)
            {
                _logger.LogWarning(
                    "External login link failed: user not found and auto-provisioning disabled. Email: {Email}",
                    request.Email);
                return new LinkExternalLoginResult(false, [ErrorMessages.UserNotFound]);
            }

            if (user == null)
            {
                var (createdUser, createErrors) = await CreateUserAsync(request.Email).ConfigureAwait(false);
                if (createErrors != null && createErrors.Length != 0)
                {
                    return new LinkExternalLoginResult(false, createErrors);
                }
                user = createdUser;
            }

            if (user == null)
            {
                // Defensive: Should not happen, but ensures non-null for AddLoginAsync
                _logger.LogWarning("Failed to create or retrieve user for external login linking. Email: {Email}", request.Email);
                return new LinkExternalLoginResult(false, [ErrorMessages.UserNotFound]);
            }

            var loginInfo = new UserLoginInfo(request.Provider, request.ProviderKey, request.ProviderDisplayName);
            var linkResult = await _userManager.AddLoginAsync(user, loginInfo).ConfigureAwait(false);

            if (!linkResult.Succeeded)
            {
                _logger.LogWarning("Failed to link external login. UserId: {UserId}, Errors: {Errors}",
                    user.Id,
                    string.Join(", ", linkResult.Errors?.Select(e => e.Description) ?? []));
                return new LinkExternalLoginResult(false, IdentityHelper.MapIdentityErrors(linkResult));
            }

            _logger.LogInformation("Successfully linked external login. Email: {Email}, Provider: {Provider}",
                request.Email, request.Provider);
            return new LinkExternalLoginResult(true);
        }

        private async Task<AuthenticateUserResult> CreateOrLinkUserWithExternalLoginAsync(
            string email,
            string providerName,
            string providerKey)
        {
            var existingUser = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);
            User user;

            if (existingUser != null)
            {
                user = existingUser;
                _logger.LogInformation("Found existing user by email. UserId: {UserId}, Email: {Email}",
                    user.Id, email);
            }
            else
            {
                if (!_configuration.AutoProvisionUser)
                {
                    _logger.LogWarning(
                        "External authentication failed: user not found and auto-provisioning disabled. Email: {Email}",
                        email);
                    return IdentityHelper.CreateFailureResult([ErrorMessages.UserNotFound]);
                }

                var (createdUser, createErrors) = await CreateUserAsync(email).ConfigureAwait(false);
                if (createErrors != null && createErrors.Length != 0)
                {
                    return IdentityHelper.CreateFailureResult(createErrors);
                }
                user = createdUser!;
            }

            var loginInfo = new UserLoginInfo(providerName, providerKey, user.Email);
            var linkResult = await _userManager.AddLoginAsync(user, loginInfo).ConfigureAwait(false);

            if (!linkResult.Succeeded)
            {
                _logger.LogWarning("Failed to link external login. UserId: {UserId}, Errors: {Errors}",
                    user.Id,
                    string.Join(", ", linkResult.Errors?.Select(e => e.Description) ?? []));
                return IdentityHelper.CreateFailureResult(IdentityHelper.MapIdentityErrors(linkResult));
            }

            _logger.LogInformation(
                "Successfully linked external login. UserId: {UserId}, Email: {Email}, Provider: {Provider}",
                user.Id, email, providerName);

            return await CreateSuccessResultAsync(user);
        }

        private async Task<AuthenticateUserResult> CreateSuccessResultAsync(User user)
        {
            ArgumentNullException.ThrowIfNull(user);
            var claimsDb = await _userManager.GetClaimsAsync(user).ConfigureAwait(false);
            var claims = ClaimHelper.MergeClaims(user, claimsDb);
            var token = _tokenGenerator.GenerateToken(claims);
            var claimDtos = ClaimHelper.MapToDto(claims);
            return new AuthenticateUserResult(true, token, claimDtos);
        }

        private async Task<(User? user, string[]? errors)> CreateUserAsync(string email)
        {
            var user = new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user).ConfigureAwait(false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User created successfully. Email: {Email}, UserId: {UserId}",
                    email, user.Id);
                return (user, null);
            }

            _logger.LogWarning("Failed to create user. Email: {Email}, Errors: {Errors}",
                email,
                string.Join(", ", result.Errors?.Select(e => e.Description) ?? []));

            return (null, IdentityHelper.MapIdentityErrors(result));
        }
    }
}