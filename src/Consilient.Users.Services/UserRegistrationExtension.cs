using Consilient.Data;
using Consilient.Users.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Users.Services
{
    public static class UserRegistrationExtension
    {
        public static IServiceCollection RegisterUserServices(this IServiceCollection services, UserServiceConfiguration userServiceConfiguration, TokenGeneratorConfiguration configuration)
        {
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
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

            services.AddScoped<IUserService>(sp => ActivatorUtilities.CreateInstance<UserService>(sp, userServiceConfiguration));
            services.AddSingleton(new TokenGenerator(configuration));
            return services;
        }
    }
}
