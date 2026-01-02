using Consilient.Users.Contracts;
using Microsoft.AspNetCore.Identity;

namespace Consilient.Users.Services.Helpers
{
    /// <summary>
    /// Helper class for common Identity operations.
    /// </summary>
    internal static class IdentityHelper
    {
        /// <summary>
        /// Creates a failure authentication result.
        /// </summary>
        public static AuthenticateUserResult CreateFailureResult(string[] errors) =>
            new(false, null, null, errors);

        /// <summary>
        /// Creates an invalid credentials authentication result.
        /// </summary>
        public static AuthenticateUserResult CreateInvalidCredentialsResult() =>
            new(false, null, null, [ErrorMessages.InvalidCredentials]);

        /// <summary>
        /// Maps IdentityResult errors to string array.
        /// </summary>
        public static string[] MapIdentityErrors(IdentityResult result) =>
            result.Errors?.Select(e => e.Description).Where(d => !string.IsNullOrWhiteSpace(d)).ToArray()
            ?? [ErrorMessages.UnexpectedError];
    }
}