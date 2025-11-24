using Consilient.Data.Configurations.Identity;
using Consilient.Data.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Data
{
    public class UsersDbContext(DbContextOptions<UsersDbContext> options) : IdentityDbContext<User, Role, int, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>(options)
    {
        internal static class Schemas
        {
            public const string Identity = "Identity";
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly, type => type.Namespace != null && type.Namespace != null && type.Namespace.StartsWith(typeof(UserConfiguration).Namespace!));
        }
    }
}
