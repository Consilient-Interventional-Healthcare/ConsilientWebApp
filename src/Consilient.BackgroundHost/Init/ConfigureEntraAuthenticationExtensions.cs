using Consilient.Users.Contracts.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Serilog;

namespace Consilient.BackgroundHost.Init
{
    internal static class ConfigureEntraAuthenticationExtensions
    {
        /// <summary>
        /// Configures Azure Entra (Azure AD) authentication for the Hangfire dashboard.
        /// Only active when OAuth is enabled in configuration AND all required settings are present.
        /// </summary>
        public static IServiceCollection ConfigureEntraAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var oauthOptions = configuration
                .GetSection(OAuthProviderOptions.SectionName)
                .Get<OAuthProviderOptions>();

            if (oauthOptions?.Enabled != true)
            {
                Log.Information("Azure Entra authentication is disabled");
                return services;
            }

            // Validate required configuration - skip if any required value is missing
            if (string.IsNullOrEmpty(oauthOptions.ClientId) ||
                string.IsNullOrEmpty(oauthOptions.TenantId) ||
                string.IsNullOrEmpty(oauthOptions.ClientSecret))
            {
                Log.Warning("Azure Entra authentication is enabled but required configuration is missing (ClientId, TenantId, or ClientSecret). Skipping authentication setup.");
                return services;
            }

            Log.Information("Configuring Azure Entra authentication for Hangfire dashboard");

            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(options =>
                {
                    options.Instance = oauthOptions.Authority ?? "https://login.microsoftonline.com/";
                    options.TenantId = oauthOptions.TenantId;
                    options.ClientId = oauthOptions.ClientId;
                    options.ClientSecret = oauthOptions.ClientSecret;
                    options.CallbackPath = "/signin-oidc";
                });

            return services;
        }
    }
}
