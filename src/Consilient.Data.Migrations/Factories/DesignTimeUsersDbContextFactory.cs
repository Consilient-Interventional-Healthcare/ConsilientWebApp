using Microsoft.EntityFrameworkCore;

namespace Consilient.Data.Migrations.Factories
{
    public class DesignTimeUsersDbContextFactory : DesignTimeDbContextFactoryBase<UsersDbContext>
    {
        protected override (string, string) GetMigrationTableAndSchema()
        {
            return ("__EFMigrationsHistory_Users", UsersDbContext.Schemas.Identity);
        }

        protected override UsersDbContext InstantiateDbContext(DbContextOptionsBuilder<UsersDbContext> optionsBuilder)
        {
            return new UsersDbContext(optionsBuilder.Options);
        }
    }
}
