namespace Consilient.Users.Services;

internal static class ErrorMessages
{
    // Authentication
    public static string InvalidCredentials { get; } = "Invalid credentials.";

    // Email / domain
    public static string EmailDomainNotAllowed { get; } = "Email domain not allowed.";

    // User provisioning / linking
    public static string UserNotFound { get; } = "User not found.";
    public static string ExternalLoginAlreadyLinked { get; } = "External login is already linked to a different account.";
    public static string ExternalLoginFailed { get; } = "Failed to link external login.";

    // Generic
    public static string UnexpectedError { get; } = "An unexpected error occurred.";
}
