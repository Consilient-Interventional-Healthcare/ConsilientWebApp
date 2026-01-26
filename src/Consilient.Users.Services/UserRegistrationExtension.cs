using Consilient.Data;
using Consilient.Data.Entities.Identity;
using Consilient.Users.Contracts;
using Consilient.Users.Contracts.OAuth;
using Consilient.Users.Services.OAuth;
using Consilient.Users.Services.OAuth.StateManagers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Consilient.Users.Services
{
    public static class UserRegistrationExtension
    {
        public static IServiceCollection RegisterUserServices(
            this IServiceCollection services,
            PasswordPolicyOptions passwordPolicy,
            bool useDistributedCache = true)
        {
            services.AddIdentity<User, Role>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = passwordPolicy.RequireDigit;
                options.Password.RequiredLength = passwordPolicy.RequiredLength;
                options.Password.RequireNonAlphanumeric = passwordPolicy.RequireNonAlphanumeric;
                options.Password.RequireUppercase = passwordPolicy.RequireUppercase;
                options.Password.RequireLowercase = passwordPolicy.RequireLowercase;
                options.Password.RequiredUniqueChars = passwordPolicy.RequiredUniqueChars;
            })
            .AddEntityFrameworkStores<UsersDbContext>()
            .AddDefaultTokenProviders();

            // Register OAuth provider services (only if enabled in configuration)
            // Build a temporary service provider to read configuration
            var serviceProvider = services.BuildServiceProvider();
            var userConfig = serviceProvider.GetService<IOptions<UserServiceOptions>>();

            // Only register OAuth service if it's configured and enabled
            if (userConfig?.Value?.OAuth?.Enabled == true)
            {
                services.AddScoped<IOAuthProviderService, MicrosoftOAuthProviderService>();
            }

            // Register OAuth provider registry
            services.AddScoped<IOAuthProviderRegistry, OAuthProviderRegistry>();

            // Register appropriate state manager based on configuration
            if (useDistributedCache)
            {
                services.AddScoped<IOAuthStateManager, DistributedOAuthStateManager>();
            }
            else
            {
                // Fallback to in-memory for development/testing
                services.AddSingleton<IOAuthStateManager, InMemoryOAuthStateManager>();
            }

            services.AddScoped<ITokenGenerator, TokenGenerator>();
            services.AddScoped<IUserService, UserService>();
            return services;
        }
    }
}
