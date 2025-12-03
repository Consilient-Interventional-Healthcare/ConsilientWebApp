using Consilient.Data;
using Consilient.Data.Entities.Identity;
using Consilient.Users.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Users.Services
{
    public static class UserRegistrationExtension
    {
        public static IServiceCollection RegisterUserServices(this IServiceCollection services, UserServiceConfiguration userServiceConfiguration, TokenGeneratorConfiguration configuration)
        {
            services.AddIdentity<User, Role>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredUniqueChars = 1;
            })
            .AddEntityFrameworkStores<UsersDbContext>()
            .AddDefaultTokenProviders();

            // Register configuration instances so DI can inject them into services
            services.AddSingleton(userServiceConfiguration);
            services.AddSingleton(configuration);

            // Register TokenGenerator instance (preserve existing behavior)
            services.AddSingleton(new TokenGenerator(configuration));

            // Let the DI container construct UserService and resolve its dependencies
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
