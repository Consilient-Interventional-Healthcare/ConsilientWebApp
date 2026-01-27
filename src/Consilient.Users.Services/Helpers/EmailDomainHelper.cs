namespace Consilient.Users.Services.Helpers;

internal static class EmailDomainHelper
{
    public static bool IsEmailDomainAllowed(string? email, IEnumerable<string>? allowedDomains)
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

        if (allowedDomains == null)
        {
            return true; // no restriction configured
        }

        // materialize to array to allow length check and repeated enumeration
        var allowed = allowedDomains as string[] ?? [.. allowedDomains];
        if (allowed.Length == 0)
        {
            return true; // no restriction configured
        }

        return allowed.Any(d => string.Equals(d?.Trim(), domain, StringComparison.OrdinalIgnoreCase));
    }
}