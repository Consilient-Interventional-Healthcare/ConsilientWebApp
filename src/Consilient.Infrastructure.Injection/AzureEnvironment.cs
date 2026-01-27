namespace Consilient.Infrastructure.Injection;

/// <summary>
/// Helper for detecting Azure App Service environment.
/// </summary>
public static class AzureEnvironment
{
    /// <summary>
    /// Returns true if the application is running in Azure App Service.
    /// WEBSITE_SITE_NAME is a read-only environment variable automatically
    /// set by Azure App Service platform - it cannot be changed by users.
    /// See: https://learn.microsoft.com/en-us/azure/app-service/reference-app-settings
    /// </summary>
    public static bool IsRunningInAzure =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
}
