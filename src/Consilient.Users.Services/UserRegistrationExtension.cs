using Consilient.Users.Contracts;
using Consilient.Users.Services.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Users.Services
{
    public static class UserRegistrationExtension
    {
        /// <summary>
        /// Registers Identity, the UsersDbContext and IUserService implementation.
        /// Caller must provide a DbContext options configuration (e.g. UseSqlServer, UseSqlite).
        /// </summary>
        public static IServiceCollection RegisterUserServices(this IServiceCollection services, Action<DbContextOptionsBuilder> dbContextOptions, TokenGeneratorConfiguration configuration)
        {
            services.AddDbContext<UsersDbContext>(dbContextOptions);

            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                // You can customize password/lockout/other options here if desired.
            })
            .AddEntityFrameworkStores<UsersDbContext>()
            .AddDefaultTokenProviders();

            services.AddScoped<IUserService, UserService>();

            services.AddSingleton(new TokenGenerator(configuration));

            return services;
        }
    }
}
