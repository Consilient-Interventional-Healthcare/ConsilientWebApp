using Consilient.Users.Contracts.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace Consilient.BackgroundHost.Init
{
    internal static class ConfigureEntraAuthenticationExtensions
    {
        /// <summary>
        /// Configures Azure Entra (Azure AD) authentication for the Hangfire dashboard.
        /// Only active when OAuth is enabled in configuration.
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
                return services;
            }

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
