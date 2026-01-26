using Consilient.BackgroundHost.Configuration;
using Consilient.Infrastructure.Injection;
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
        /// Determines if Entra authentication should be used.
        /// Returns true if (running in Azure OR ForceEntraAuth is enabled) AND OAuth is properly configured.
        /// </summary>
        public static bool ShouldUseEntraAuth(IConfiguration configuration)
        {
            var authSettings = configuration
                .GetSection(AuthenticationSettings.SectionName)
                .Get<AuthenticationSettings>();

            // First check: must be in Azure OR ForceEntraAuth must be true
            var environmentRequiresAuth = AzureEnvironment.IsRunningInAzure || (authSettings?.ForceEntraAuth == true);
            if (!environmentRequiresAuth)
            {
                return false;
            }

            // Second check: OAuth must be enabled and properly configured
            var oauthOptions = configuration
                .GetSection(OAuthProviderOptions.SectionName)
                .Get<OAuthProviderOptions>();

            if (oauthOptions?.Enabled != true)
            {
                return false;
            }

            // Third check: required values must be present
            if (string.IsNullOrEmpty(oauthOptions.ClientId) ||
                string.IsNullOrEmpty(oauthOptions.TenantId) ||
                string.IsNullOrEmpty(oauthOptions.ClientSecret))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Configures Azure Entra (Azure AD) authentication for the Hangfire dashboard.
        /// Only active when running in Azure (or ForceEntraAuth is true) AND OAuth is enabled in configuration.
        /// </summary>
        public static IServiceCollection ConfigureEntraAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Check if we should use Entra auth (includes all validation)
            if (!ShouldUseEntraAuth(configuration))
            {
                Log.Information("Entra authentication disabled - either not in Azure/ForceEntraAuth, or OAuth not properly configured");
                return services;
            }

            var oauthOptions = configuration
                .GetSection(OAuthProviderOptions.SectionName)
                .Get<OAuthProviderOptions>()!; // Safe - ShouldUseEntraAuth already validated

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

            services.AddAuthorization(options =>
            {
                options.AddPolicy("HangfireAccess", builder =>
                {
                    builder.AddAuthenticationSchemes(OpenIdConnectDefaults.AuthenticationScheme)
                           .RequireAuthenticatedUser();
                });
            });

            return services;
        }
    }
}
