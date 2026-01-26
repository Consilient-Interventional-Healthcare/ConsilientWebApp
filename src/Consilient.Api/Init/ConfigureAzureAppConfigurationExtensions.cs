using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Consilient.Api.Configuration;
using Consilient.Infrastructure.Injection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Serilog;

namespace Consilient.Api.Init
{
    internal static class ConfigureAzureAppConfigurationExtensions
    {
        private const string KeyPrefix = "ConsilientApi:";

        /// <summary>
        /// Adds Azure App Configuration as the primary configuration source for Consilient.Api.
        /// Falls back to Azure Key Vault if AAC connection fails.
        /// </summary>
        public static IConfigurationBuilder AddApiAzureAppConfiguration(
            this IConfigurationBuilder configuration,
            string environmentName,
            bool isProduction)
        {
            // Skip Azure App Configuration entirely when running locally
            if (!AzureEnvironment.IsRunningInAzure)
            {
                return configuration;
            }

            var tempConfig = configuration.Build();
            var appConfigEndpoint = tempConfig[$"{AppConfigurationOptions.SectionName}:Endpoint"];

            if (!string.IsNullOrEmpty(appConfigEndpoint))
            {
                try
                {
                    var credential = new DefaultAzureCredential();

                    // Map ASP.NET Core environment names to App Configuration labels
                    // Development -> dev, Production -> prod (matches Terraform var.environment)
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
                            // Load keys with ConsilientApi: prefix (shared across all environments)
                            .Select($"{KeyPrefix}*", LabelFilter.Null)
                            // Load keys matching environment label (dev, prod)
                            .Select($"{KeyPrefix}*", appConfigLabel)
                            // Strip the ConsilientApi: prefix so keys match expected configuration paths
                            // e.g., "ConsilientApi:ApplicationSettings:..." becomes "ApplicationSettings:..."
                            .TrimKeyPrefix(KeyPrefix)
                            // Configure Key Vault reference resolution (AAC resolves KV references at read time)
                            .ConfigureKeyVault(kv =>
                            {
                                kv.SetCredential(credential);
                            });

                        // Enable dynamic configuration refresh for production only
                        // Dev uses Free SKU and stop/start for config changes
                        if (isProduction)
                        {
                            options.ConfigureRefresh(refresh =>
                            {
                                refresh.Register($"{KeyPrefix}RefreshSentinel", refreshAll: true)
                                       .SetRefreshInterval(TimeSpan.FromMinutes(5));
                            });
                        }
                    });

                    Log.Information("Azure App Configuration loaded successfully from {Endpoint} with label {Label}", appConfigEndpoint, appConfigLabel);
                }
                catch (Exception ex)
                {
                    // If AAC connection fails, fall back to legacy Key Vault integration
                    Log.Warning(ex, "Failed to load Azure App Configuration, falling back to Azure Key Vault");
                    AddKeyVaultFallback(configuration, tempConfig);
                }
            }
            else
            {
                // LEGACY: Fallback to direct Key Vault integration if AppConfiguration endpoint not configured
                // This supports deployments without App Configuration (local dev, legacy environments, etc.)
                AddKeyVaultFallback(configuration, tempConfig);
            }

            return configuration;
        }

        private static void AddKeyVaultFallback(IConfigurationBuilder configuration, IConfiguration tempConfig)
        {
            var keyVaultUrl = tempConfig[$"{AzureKeyVaultOptions.SectionName}:Url"];
            if (!string.IsNullOrEmpty(keyVaultUrl))
            {
                var credential = new DefaultAzureCredential();
                configuration.AddAzureKeyVault(
                    new Uri(keyVaultUrl),
                    credential,
                    new KeyVaultSecretManager());
                Log.Information("Azure Key Vault loaded from {Url}", keyVaultUrl);
            }
        }
    }
}
