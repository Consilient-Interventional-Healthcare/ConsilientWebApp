namespace Consilient.Api.Configuration
{
    /// <summary>
    /// Configuration options for Azure App Configuration service.
    /// </summary>
    public class AppConfigurationOptions
    {
        public const string SectionName = "AppConfiguration";

        /// <summary>
        /// The Azure App Configuration endpoint URL.
        /// </summary>
        public string? Endpoint { get; init; }
    }

    /// <summary>
    /// Configuration options for Azure Key Vault.
    /// </summary>
    public class AzureKeyVaultOptions
    {
        public const string SectionName = "KeyVault";

        /// <summary>
        /// The Azure Key Vault URL.
        /// </summary>
        public string? Url { get; init; }
    }
}
