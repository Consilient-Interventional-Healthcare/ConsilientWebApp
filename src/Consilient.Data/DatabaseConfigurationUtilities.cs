using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Consilient.Data;

public static class DatabaseConfigurationUtilities
{
    public static DbContextOptionsBuilder ConfigureDataContext(this DbContextOptionsBuilder builder, string? connectionString, bool isProduction, string? migrationsHistoryTable = null, string? migrationsHistorySchema = null)
    {
        static void options(SqlServerDbContextOptionsBuilder sqlOptions, string? migrationsHistoryTable, string? migrationsHistorySchema)
        {
            sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            sqlOptions.EnableRetryOnFailure();
            sqlOptions.MigrationsAssembly($"{typeof(DatabaseConfigurationUtilities).Namespace}.Migrations");

            if (!string.IsNullOrEmpty(migrationsHistoryTable))
            {
                // Configure a dedicated migrations history table (and optional schema) for this DbContext
                sqlOptions.MigrationsHistoryTable(migrationsHistoryTable, migrationsHistorySchema);
            }
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            builder.UseSqlServer(sqlOptions => options(sqlOptions, migrationsHistoryTable, migrationsHistorySchema));
        }
        else
        {
            builder.UseSqlServer(connectionString, sqlOptions => options(sqlOptions, migrationsHistoryTable, migrationsHistorySchema));
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
