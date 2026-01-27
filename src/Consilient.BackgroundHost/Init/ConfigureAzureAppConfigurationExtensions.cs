using Azure.Identity;
using Consilient.Infrastructure.Injection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace Consilient.BackgroundHost.Init;

internal static class ConfigureAzureAppConfigurationExtensions
{
    private const string AppConfigurationEndpointKey = "AppConfiguration:Endpoint";
    private const string KeyPrefix = "BackgroundHost:";

    /// <summary>
    /// Adds Azure App Configuration as the primary configuration source for BackgroundHost.
    /// Falls back gracefully to local config if AAC is unavailable.
    /// </summary>
    public static IConfigurationBuilder AddBackgroundHostAzureAppConfiguration(
        this IConfigurationBuilder configuration,
        string environmentName)
    {
        // Skip Azure App Configuration entirely when running locally
        if (!AzureEnvironment.IsRunningInAzure)
        {
            return configuration;
        }

        var tempConfig = configuration.Build();
        var appConfigEndpoint = tempConfig[AppConfigurationEndpointKey];

        if (string.IsNullOrEmpty(appConfigEndpoint))
        {
            return configuration;
        }

        try
        {
            var credential = new DefaultAzureCredential();
            var appConfigLabel = environmentName switch
            {
                "Development" => "dev",
                "Production" => "prod",
                _ => environmentName.ToLower()
            };

            configuration.AddAzureAppConfiguration(options =>
            {
                options
                    .Connect(new Uri(appConfigEndpoint), credential)
                    .Select($"{KeyPrefix}*", LabelFilter.Null)
                    .Select($"{KeyPrefix}*", appConfigLabel)
                    .TrimKeyPrefix(KeyPrefix)
                    .ConfigureKeyVault(kv =>
                    {
                        kv.SetCredential(credential);
                    });
            });
        }
        catch (Exception ex)
        {
            // Log warning and continue with local config if App Configuration fails
            Console.WriteLine($"Warning: Failed to load Azure App Configuration: {ex.Message}");
        }

        return configuration;
    }
}
