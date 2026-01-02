using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Consilient.Data.Migrations.Factories
{
    public abstract class DesignTimeDbContextFactoryBase<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
        where TDbContext : DbContext
    {
        public TDbContext CreateDbContext(string[] args)
        {
            var connectionString = string.Empty;
            var connectionArgIndex = Array.IndexOf(args, "--connection");
            if (connectionArgIndex != -1 && connectionArgIndex < args.Length - 1)
            {
                connectionString = args[connectionArgIndex + 1];
            }
            var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
            optionsBuilder.ConfigureDataContext(connectionString, false);
            return InstantiateDbContext(optionsBuilder);
        }

        protected abstract (string, string) GetMigrationTableAndSchema();

        protected virtual TDbContext InstantiateDbContext(DbContextOptionsBuilder<TDbContext> optionsBuilder)
        {
            return (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)!;
        }
    }
}
