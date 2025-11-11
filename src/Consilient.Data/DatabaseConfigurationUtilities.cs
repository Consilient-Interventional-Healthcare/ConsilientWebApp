using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Consilient.Data
{
    public static class DatabaseConfigurationUtilities
    {
        public static DbContextOptionsBuilder ConfigureDataContext(this DbContextOptionsBuilder builder, string? connectionString, bool isProduction)
        {
            static void options(SqlServerDbContextOptionsBuilder sqlOptions)
            {
                sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                sqlOptions.EnableRetryOnFailure();
                sqlOptions.MigrationsAssembly($"{typeof(DatabaseConfigurationUtilities).Namespace}.Migrations");
            }
            if (string.IsNullOrEmpty(connectionString))
            {
                builder.UseSqlServer(options);
            }
            else
            {
                builder.UseSqlServer(connectionString, options);
            }
            builder.ConfigureWarnings(w =>
            {
                if (isProduction)
                {
                    w.Ignore(RelationalEventId.MultipleCollectionIncludeWarning);
                }
                else
                {
                    w.Throw(RelationalEventId.MultipleCollectionIncludeWarning);
                }
            });
            return builder;
        }
    }
}
