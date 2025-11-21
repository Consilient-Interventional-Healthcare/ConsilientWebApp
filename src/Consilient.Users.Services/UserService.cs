using Consilient.Users.Contracts;
using Microsoft.AspNetCore.Identity;

namespace Consilient.Users.Services
{
    public class UserService(UserServiceConfiguration configuration, UserManager<IdentityUser> userManager, TokenGeneratorConfiguration tokenGeneratorConfiguration) : IUserService
    {
        private readonly TokenGenerator _tokenGenerator = new(tokenGeneratorConfiguration);

        //public async Task<CreateUserResult> CreateUserAsync(CreateUserRequest request)
        //{
        //    var user = new IdentityUser
        //    {
        //        UserName = request.Email,
        //        Email = request.Email
        //    };

        //    var result = await userManager.CreateAsync(user, request.Password).ConfigureAwait(false);

        //    return new CreateUserResult(result.Succeeded, result.Errors.Select(e => e.Description));
        //}

        public async Task<AuthenticateUserResult> AuthenticateUserAsync(AuthenticateUserRequest request)
        {
            // Validate credentials without creating a sign-in session
            var user = await userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);
            if (user == null)
            {
                return new AuthenticateUserResult(false, null, ["Invalid credentials."]);
            }

            var passwordValid = await userManager.CheckPasswordAsync(user, request.Password).ConfigureAwait(false);
            if (!passwordValid)
            {
                return new AuthenticateUserResult(false, null, ["Invalid credentials."]);
            }

            // generate JWT using token generator
            var tokenString = _tokenGenerator.GenerateToken(user);

            return new AuthenticateUserResult(true, tokenString);
        }

        public async Task<LinkExternalLoginResult> LinkExternalLoginAsync(LinkExternalLoginRequest request)
        {
            var user = await userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);
            if (user == null && configuration.AutoProvisionUser)
            {
                // auto-provision a local user record when not found
                user = new IdentityUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user).ConfigureAwait(false);
                if (!createResult.Succeeded)
                {
                    return new LinkExternalLoginResult(false, createResult.Errors.Select(e => e.Description));
                }
            }

            if (user == null)
            {
                // User not found and auto-provision is disabled, return error
                return new LinkExternalLoginResult(false, ["User not found."]);
            }

            var userLogin = new UserLoginInfo(request.Provider, request.ProviderKey, request.ProviderDisplayName);
            var result = await userManager.AddLoginAsync(user, userLogin).ConfigureAwait(false);

            return new LinkExternalLoginResult(result.Succeeded, result.Errors.Select(e => e.Description));
        }
    }
}
