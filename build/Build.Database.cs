using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

/// <summary>
/// Database and EF Core migration targets.
/// Handles: Database health checks, migrations, SQL script generation, and database reset.
/// </summary>
partial class Build
{
    // Database container configuration
    const string DatabaseContainerName = "consilient.dbs.container";
    const string DatabaseConnectionString = "Server=localhost,1434;Database=consilient_main;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";

    // Database paths (MigrationsProject is defined in Build.cs)
    static AbsolutePath DatabaseScriptsDir => SourceDirectory / "Databases";

    // Parameters
    [Parameter("Migration name (required for AddMigration)")]
    readonly string? MigrationName;

    [Parameter("Override sequence number for SQL script (1-99)")]
    readonly int? SequenceNumber;

    [Parameter("Target database name")]
    readonly string Database = "consilient_main";

    // ============================================
    // DATABASE HEALTH TARGETS
    // ============================================

    Target CheckDatabaseHealth => _ => _
        .Description("Verify database container is running and healthy")
        .Executes(() =>
        {
            Log.Information("Checking database container health...");

            var process = ProcessTasks.StartProcess(
                "docker",
                $"inspect {DatabaseContainerName} --format=\"{{{{.State.Health.Status}}}}\"",
                workingDirectory: RootDirectory);

            if (process.ExitCode != 0)
            {
                Log.Error("Database container '{Container}' not found. Start it with: docker compose up -d db", DatabaseContainerName);
                throw new Exception($"Database container '{DatabaseContainerName}' not found");
            }

            var output = string.Join("", process.Output.Select(o => o.Text)).Trim().Trim('"');
            if (output != "healthy")
            {
                Log.Warning("Database container status: {Status}. Expected: healthy", output);
                Log.Warning("Start the database with: cd src/.docker && docker compose up -d db");
                throw new Exception($"Database container is not healthy. Current status: {output}");
            }

            Log.Information("Database container is healthy");
        });

    // ============================================
    // MIGRATION TARGETS
    // ============================================

    Target UpdateLocalDatabase => _ => _
        .DependsOn(Compile)
        .Description("Apply pending EF Core migrations to local database")
        .OnlyWhenDynamic(() => !SkipDatabase)
        .Executes(() =>
        {
            var contexts = DbContext == "Both"
                ? new[] { "ConsilientDbContext", "UsersDbContext" }
                : new[] { DbContext };

            foreach (var ctx in contexts)
            {
                Log.Information("Applying migrations for {Context}...", ctx);

                DotNet(
                    $"ef database update --context {ctx} --project \"{MigrationsProject}\" --startup-project \"{MigrationsProject}\" --connection \"{DatabaseConnectionString}\" --verbose",
                    workingDirectory: SourceDirectory);

                Log.Information("Migrations applied successfully for {Context}", ctx);
            }
        });

    Target CheckMigrations => _ => _
        .DependsOn(Compile)
        .Description("List pending EF Core migrations")
        .Executes(() =>
        {
            var contexts = DbContext == "Both"
                ? new[] { "ConsilientDbContext", "UsersDbContext" }
                : new[] { DbContext };

            foreach (var ctx in contexts)
            {
                Log.Information("Checking pending migrations for {Context}...", ctx);

                DotNet(
                    $"ef migrations list --context {ctx} --project \"{MigrationsProject}\" --startup-project \"{MigrationsProject}\" --connection \"{DatabaseConnectionString}\"",
                    workingDirectory: SourceDirectory);
            }
        });

    Target EnsureDatabase => _ => _
        .Description("Start database container (if needed) and apply migrations")
        .Executes(() =>
        {
            // Step 1: Check if container exists and is running
            Log.Information("Checking if database container is running...");

            var inspectProcess = ProcessTasks.StartProcess(
                "docker",
                $"inspect {DatabaseContainerName} --format=\"{{{{.State.Running}}}}\"",
                workingDirectory: RootDirectory);

            bool containerRunning = inspectProcess.ExitCode == 0 &&
                string.Join("", inspectProcess.Output.Select(o => o.Text)).Contains("true");

            // Step 2: Start container if not running
            if (!containerRunning)
            {
                Log.Information("Starting database container...");
                RunDocker($"compose -f \"{DockerComposeFile}\" up -d db", DockerDirectory);
            }
            else
            {
                Log.Information("Database container is already running");
            }

            // Step 3: Wait for healthy status
            WaitForDatabaseHealthy(retries: 15, intervalSeconds: 5);

            // Step 4: Apply migrations
            Log.Information("Applying database migrations...");
            var contexts = new[] { "ConsilientDbContext", "UsersDbContext" };

            foreach (var ctx in contexts)
            {
                Log.Information("Applying migrations for {Context}...", ctx);
                DotNet(
                    $"ef database update --context {ctx} --project \"{MigrationsProject}\" --startup-project \"{MigrationsProject}\" --connection \"{DatabaseConnectionString}\"",
                    workingDirectory: SourceDirectory);
            }

            Log.Information("Database is ready and up-to-date");
        });

    Target AddMigration => _ => _
        .DependsOn(Compile)
        .Description("Add a new EF Core migration")
        .Requires(() => MigrationName)
        .Requires(() => DbContext != "Both")
        .Executes(() =>
        {
            var context = DbContext;

            // Convention-based derivation (e.g., "ConsilientDbContext" -> "Consilient")
            var contextShort = Regex.Match(context, @"^(.*)DbContext$").Groups[1].Value;
            if (string.IsNullOrEmpty(contextShort))
                contextShort = context;

            var migrationNamespace = $"Consilient.Data.Migrations.{contextShort}";
            var outputDir = contextShort;
            var fullOutputDir = MigrationsProject / outputDir;
            var snapshotFile = fullOutputDir / $"{context}ModelSnapshot.cs";

            Log.Information("Adding migration '{MigrationName}' for {Context}...", MigrationName, context);

            // Ensure output directory exists
            fullOutputDir.CreateDirectory();

            // Ensure snapshot file exists so EF Core places it in the correct location
            if (!snapshotFile.FileExists())
            {
                Log.Information("Creating placeholder snapshot file to ensure correct output location...");
                var snapshotContent = $@"// <auto-generated />
using Consilient.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace {migrationNamespace}
{{
    [DbContext(typeof({context}))]
    partial class {context}ModelSnapshot : ModelSnapshot
    {{
        protected override void BuildModel(ModelBuilder modelBuilder)
        {{
        }}
    }}
}}
";
                File.WriteAllText(snapshotFile, snapshotContent);
            }

            // Run dotnet ef migrations add
            DotNet(
                $"ef migrations add {MigrationName} --context {context} --project \"{MigrationsProject}\" --startup-project \"{MigrationsProject}\" --output-dir {outputDir} --namespace {migrationNamespace} --verbose",
                workingDirectory: SourceDirectory);

            Log.Information("Migration '{MigrationName}' added successfully for {Context}", MigrationName, context);
        });

    Target GenerateMigrationScript => _ => _
        .DependsOn(Compile)
        .Description("Generate SQL migration script from latest EF migration")
        .Executes(() =>
        {
            var contexts = DbContext == "Both"
                ? new[] { "ConsilientDbContext", "UsersDbContext" }
                : new[] { DbContext };

            foreach (var ctx in contexts)
            {
                // Convention-based derivation
                var contextShort = Regex.Match(ctx, @"^(.*)DbContext$").Groups[1].Value;
                if (string.IsNullOrEmpty(contextShort))
                    contextShort = ctx;

                var migrationsDir = MigrationsProject / contextShort;
                var outputDir = DatabaseScriptsDir / $"{contextShort.ToLower()}_main";

                Log.Information("Generating migration script for {Context}...", ctx);

                // Ensure output directory exists
                outputDir.CreateDirectory();

                // Find the next sequence number based on existing files
                int nextNumber;
                if (SequenceNumber.HasValue)
                {
                    nextNumber = SequenceNumber.Value;
                }
                else
                {
                    var existingFiles = Directory.GetFiles(outputDir, "*.sql")
                        .Select(Path.GetFileName)
                        .Where(f => Regex.IsMatch(f!, @"^\d{2}_"))
                        .OrderByDescending(f => f)
                        .ToList();

                    if (existingFiles.Count > 0)
                    {
                        var lastNumber = int.Parse(existingFiles[0]!.Substring(0, 2));
                        nextNumber = lastNumber + 1;
                    }
                    else
                    {
                        nextNumber = 1;
                    }
                }
                var prefix = nextNumber.ToString("D2");

                // Get all migrations sorted by name
                var allMigrations = Directory.GetFiles(migrationsDir, "*.cs")
                    .Select(Path.GetFileName)
                    .Where(f => Regex.IsMatch(f!, @"^\d{14}_.*\.cs$") && !f!.Contains(".Designer."))
                    .OrderByDescending(f => f)
                    .ToList();

                if (allMigrations.Count == 0)
                {
                    Log.Warning("No migrations found for {Context}", ctx);
                    continue;
                }

                var latestMigration = allMigrations[0]!;
                var latestMigrationName = Path.GetFileNameWithoutExtension(latestMigration);
                var migrationDisplayName = Regex.Replace(latestMigrationName, @"^\d{14}_", "");

                // Determine the 'from' migration
                string fromMigration;
                if (allMigrations.Count > 1)
                {
                    fromMigration = Path.GetFileNameWithoutExtension(allMigrations[1]!);
                }
                else
                {
                    fromMigration = "0";
                }

                var outputFile = outputDir / $"{prefix}_{migrationDisplayName}.sql";

                Log.Information("  From: {From}", fromMigration);
                Log.Information("  To:   {To}", latestMigrationName);

                DotNet(
                    $"ef migrations script {fromMigration} {latestMigrationName} --idempotent --context {ctx} --project \"{MigrationsProject}\" --startup-project \"{MigrationsProject}\" --output \"{outputFile}\"",
                    workingDirectory: SourceDirectory);

                Log.Information("Migration script saved to: {File}", outputFile);
            }

            Log.Information("Script generation completed successfully");
        });

    Target ResetDatabase => _ => _
        .Description("Reset local database (destroys all data)")
        .Requires(() => Force)
        .Executes(() =>
        {
            Log.Warning("Resetting database '{Database}'...", Database);

            // Rebuild the database image to ensure latest SQL scripts are included
            Log.Information("Rebuilding database image with latest scripts...");
            RunDocker($"compose -f \"{DockerComposeFile}\" build --no-cache db", DockerDirectory);

            // Remove existing container to avoid naming conflicts
            Log.Information("Removing existing database container...");
            var removeProcess = ProcessTasks.StartProcess(
                "docker",
                $"rm -f {DatabaseContainerName}",
                workingDirectory: RootDirectory);
            // Ignore exit code - container may not exist

            // Ensure the container is running first
            Log.Information("Starting database container...");
            RunDocker($"compose -f \"{DockerComposeFile}\" up -d db", DockerDirectory);
            Thread.Sleep(TimeSpan.FromSeconds(2));

            // Create marker file on the persistent volume
            Log.Information("Creating reset marker for {Database}...", Database);
            RunDocker($"compose -f \"{DockerComposeFile}\" exec -T db bash -c \"touch /var/opt/mssql/.reset-{Database}\"", DockerDirectory);

            // Restart the db service to trigger entrypoint with the marker
            Log.Information("Restarting database container...");
            RunDocker($"compose -f \"{DockerComposeFile}\" up -d --force-recreate db", DockerDirectory);

            // Wait for database to be healthy
            WaitForDatabaseHealthy(retries: 24, intervalSeconds: 5);

            // Verify table creation
            Log.Information("Verifying table creation...");
            var verifyProcess = ProcessTasks.StartProcess(
                "docker",
                $"exec {DatabaseContainerName} bash -c \"echo 'SELECT COUNT(*) FROM {Database}.INFORMATION_SCHEMA.TABLES' | /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -C -h -1\"",
                workingDirectory: RootDirectory);

            if (verifyProcess.ExitCode == 0)
            {
                var output = string.Join("", verifyProcess.Output.Select(o => o.Text)).Trim();
                if (int.TryParse(output, out int tableCount))
                {
                    if (tableCount < 10)
                    {
                        Log.Warning("Only {TableCount} tables created. Check container logs for SQL errors.", tableCount);
                    }
                    else
                    {
                        Log.Information("SUCCESS: {TableCount} tables created in {Database}", tableCount, Database);
                    }
                }
            }

            Log.Information("Database {Database} has been reset", Database);
        });

    // ============================================
    // HELPER METHODS
    // ============================================

    void WaitForDatabaseHealthy(int retries = 12, int intervalSeconds = 5)
    {
        Log.Information("Waiting for database to be healthy (max {Retries} attempts, {Interval}s interval)...",
            retries, intervalSeconds);

        for (int i = 1; i <= retries; i++)
        {
            var process = ProcessTasks.StartProcess(
                "docker",
                $"inspect {DatabaseContainerName} --format=\"{{{{.State.Health.Status}}}}\"",
                workingDirectory: RootDirectory);

            if (process.ExitCode == 0)
            {
                var output = string.Join("", process.Output.Select(o => o.Text)).Trim().Trim('"');
                if (output == "healthy")
                {
                    Log.Information("Database is healthy after {Attempts} attempt(s)", i);
                    return;
                }
                Log.Debug("Health check attempt {Current}/{Max}: {Status}", i, retries, output);
            }
            else
            {
                Log.Debug("Health check attempt {Current}/{Max}: Container not ready", i, retries);
            }

            if (i < retries)
            {
                Thread.Sleep(TimeSpan.FromSeconds(intervalSeconds));
            }
        }

        throw new Exception($"Database did not become healthy after {retries} attempts ({retries * intervalSeconds}s)");
    }
}
