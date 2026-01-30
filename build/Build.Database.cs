using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Serilog;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

/// <summary>
/// Database and EF Core migration targets.
/// Handles: Database health checks, migrations, SQL script generation, and database reset.
/// </summary>
partial class Build
{
    // Database container configuration
    const string DatabaseContainerName = "consilient.dbs.container";

    // Database paths (MigrationsProject is defined in Build.cs)
    static AbsolutePath DatabaseScriptsDir => SourceDirectory / "Databases";
    static AbsolutePath EnvLocalFile => RootDirectory / ".nuke" / ".env.local";

    // Parameters
    [Parameter("Migration name (required for AddMigration)")]
    readonly string? MigrationName;

    [Parameter("Override sequence number for SQL script (1-99)")]
    readonly int? SequenceNumber;

    [Parameter("Target database name")]
    readonly string Database = "consilient_main";

    [Parameter("Database server host")]
    readonly string? DbHost;

    [Parameter("Database server port")]
    readonly int? DbPort;

    [Parameter("Database username")]
    readonly string? DbUser;

    [Parameter("Database password")]
    readonly string? DbPassword;

    [Parameter("Database runs in a Docker container")]
    readonly bool? DbDocker;

    [Parameter("Docker container name")]
    readonly string? DbContainerName;

    [Parameter("Path to docker-compose file (relative to root)")]
    readonly string? DbComposeFile;

    [Parameter("Docker service name in compose file")]
    readonly string? DbServiceName;

    [Parameter("Auto-start container if not running, stop after completion")]
    readonly bool? DbAutoStart;

    [Parameter("Create backup before rebuild (default: false)")]
    readonly bool Backup = false;

    // ============================================
    // DATABASE CONFIGURATION
    // ============================================

    Dictionary<string, string>? _envLocalCache;
    Dictionary<string, string> EnvLocal => _envLocalCache ??= LoadEnvLocal();

    Dictionary<string, string> LoadEnvLocal()
    {
        var env = new Dictionary<string, string>();
        if (!EnvLocalFile.FileExists())
        {
            Log.Debug("Environment file not found at {Path}, using defaults", EnvLocalFile);
            return env;
        }

        foreach (var line in File.ReadAllLines(EnvLocalFile))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
            var idx = line.IndexOf('=');
            if (idx > 0)
                env[line[..idx].Trim()] = line[(idx + 1)..].Trim();
        }

        Log.Debug("Loaded {Count} variables from {Path}", env.Count, EnvLocalFile);
        return env;
    }

    string ResolvedDbHost => DbHost ?? "localhost";
    int ResolvedDbPort => DbPort ?? 1434;
    string ResolvedDbUser => DbUser ?? EnvLocal.GetValueOrDefault("SQL_ADMIN_USERNAME", "sa");
    string ResolvedDbPassword => DbPassword ?? EnvLocal.GetValueOrDefault("SQL_ADMIN_PASSWORD", "YourStrong!Passw0rd");

    string ResolvedConnectionString =>
        $"Server={ResolvedDbHost},{ResolvedDbPort};Database={Database};User Id={ResolvedDbUser};Password={ResolvedDbPassword};TrustServerCertificate=True;";

    // Docker configuration resolved properties
    bool ResolvedDbDocker => DbDocker ?? bool.TryParse(EnvLocal.GetValueOrDefault("DB_DOCKER", "true"), out var v) && v;
    string ResolvedDbContainerName => DbContainerName ?? EnvLocal.GetValueOrDefault("DB_CONTAINER_NAME", "consilient.dbs.container");
    AbsolutePath ResolvedDbComposeFile => RootDirectory / (DbComposeFile ?? EnvLocal.GetValueOrDefault("DB_COMPOSE_FILE", "src/.docker/docker-compose.yml"));
    string ResolvedDbServiceName => DbServiceName ?? EnvLocal.GetValueOrDefault("DB_SERVICE_NAME", "db");
    bool ResolvedDbAutoStart => DbAutoStart ?? bool.TryParse(EnvLocal.GetValueOrDefault("DB_AUTO_START", "false"), out var v) && v;

    // Track if we started the container so we know to stop it
    bool _containerStartedByBuild = false;

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
            process.WaitForExit();

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
                    $"ef database update --context {ctx} --project \"{MigrationsProject}\" --startup-project \"{MigrationsProject}\" --connection \"{ResolvedConnectionString}\" --verbose",
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
                    $"ef migrations list --context {ctx} --project \"{MigrationsProject}\" --startup-project \"{MigrationsProject}\" --connection \"{ResolvedConnectionString}\"",
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
            inspectProcess.WaitForExit();

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
                    $"ef database update --context {ctx} --project \"{MigrationsProject}\" --startup-project \"{MigrationsProject}\" --connection \"{ResolvedConnectionString}\"",
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

                // New naming convention: {NN}_{context}_{MigrationName}.sql
                var outputFile = outputDir / $"{prefix}_{contextShort.ToLower()}_{migrationDisplayName}.sql";

                Log.Information("  From: {From}", fromMigration);
                Log.Information("  To:   {To}", latestMigrationName);

                DotNet(
                    $"ef migrations script {fromMigration} {latestMigrationName} --idempotent --context {ctx} --project \"{MigrationsProject}\" --startup-project \"{MigrationsProject}\" --output \"{outputFile}\"",
                    workingDirectory: SourceDirectory);

                // Prepend metadata header to generated script
                PrependMigrationScriptHeader(outputFile, ctx, latestMigrationName);

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
            removeProcess.WaitForExit();
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
            verifyProcess.WaitForExit();

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

    /// <summary>
    /// Consolidate all EF migrations into a single Initial migration.
    /// Preserves manually-created files (pattern: *_manual_*.sql) and files from other contexts.
    /// </summary>
    /// <remarks>
    /// <para><b>Windows Usage:</b></para>
    /// <code>
    /// .\build.ps1 SquashMigrations --db-context ConsilientDbContext --force
    /// </code>
    ///
    /// <para><b>Parameters:</b></para>
    /// <list type="bullet">
    ///   <item><c>--db-context</c> (required): ConsilientDbContext or UsersDbContext (not "Both")</item>
    ///   <item><c>--force</c> (required): Confirmation flag to prevent accidental execution</item>
    /// </list>
    ///
    /// <para><b>What gets deleted:</b></para>
    /// <list type="bullet">
    ///   <item>C# migration files: src/Consilient.Data.Migrations/{Context}/*.cs</item>
    ///   <item>SQL scripts matching: {NN}_{context}_*.sql</item>
    /// </list>
    ///
    /// <para><b>What gets preserved:</b></para>
    /// <list type="bullet">
    ///   <item>Manual scripts: {NN}_manual_*.sql</item>
    ///   <item>Other context scripts: {NN}_{otherContext}_*.sql</item>
    ///   <item>Seed data: seed.sql</item>
    /// </list>
    /// </remarks>
    Target SquashMigrations => _ => _
        .DependsOn(Compile)
        .Description("Consolidate all EF migrations into a single Initial migration (files only, no DB changes)")
        .Requires(() => Force)
        .Requires(() => DbContext != "Both")
        .Executes(() =>
        {
            var context = DbContext;

            // Convention-based derivation (e.g., "ConsilientDbContext" -> "Consilient" -> "consilient")
            var contextShort = Regex.Match(context, @"^(.*)DbContext$").Groups[1].Value;
            if (string.IsNullOrEmpty(contextShort))
                contextShort = context;

            var contextShortLower = contextShort.ToLower();
            var migrationsDir = MigrationsProject / contextShort;
            var sqlDir = DatabaseScriptsDir / $"{contextShortLower}_main";

            Log.Warning("=== SQUASHING MIGRATIONS FOR {Context} ===", context);
            Log.Warning("This will delete all migration files and create a fresh Initial migration.");
            Log.Information("Migrations directory: {Dir}", migrationsDir);
            Log.Information("SQL scripts directory: {Dir}", sqlDir);

            // Step 1: Delete C# migration files
            Log.Information("");
            Log.Information("Step 1: Deleting C# migration files...");

            if (Directory.Exists(migrationsDir))
            {
                var migrationFiles = Directory.GetFiles(migrationsDir, "*.cs")
                    .Where(f => Regex.IsMatch(Path.GetFileName(f)!, @"^\d{14}_"))
                    .ToList();

                foreach (var file in migrationFiles)
                {
                    Log.Information("  Deleting: {File}", Path.GetFileName(file));
                    File.Delete(file);
                }

                Log.Information("  Deleted {Count} C# migration file(s)", migrationFiles.Count);

                // Delete snapshot file
                var snapshotFile = migrationsDir / $"{context}ModelSnapshot.cs";
                if (File.Exists(snapshotFile))
                {
                    Log.Information("  Deleting snapshot: {File}", Path.GetFileName(snapshotFile));
                    File.Delete(snapshotFile);
                }
            }
            else
            {
                Log.Warning("  Migrations directory not found: {Dir}", migrationsDir);
            }

            // Step 2: Delete SQL files for this context only
            Log.Information("");
            Log.Information("Step 2: Deleting SQL scripts for {Context}...", context);

            if (Directory.Exists(sqlDir))
            {
                // Match files with naming convention: {NN}_{context}_*.sql
                var sqlPattern = $@"^\d{{2}}_{contextShortLower}_.*\.sql$";
                var sqlFiles = Directory.GetFiles(sqlDir, "*.sql")
                    .Where(f =>
                    {
                        var fileName = Path.GetFileName(f)!;
                        // Match by naming convention OR by header detection
                        return Regex.IsMatch(fileName, sqlPattern, RegexOptions.IgnoreCase) ||
                               IsAutoGeneratedMigrationScript(f, context);
                    })
                    .ToList();

                foreach (var file in sqlFiles)
                {
                    Log.Information("  Deleting: {File}", Path.GetFileName(file));
                    File.Delete(file);
                }

                Log.Information("  Deleted {Count} SQL script(s)", sqlFiles.Count);

                // List preserved files
                var preservedFiles = Directory.GetFiles(sqlDir, "*.sql")
                    .Select(Path.GetFileName)
                    .ToList();

                if (preservedFiles.Count > 0)
                {
                    Log.Information("");
                    Log.Information("  Preserved files:");
                    foreach (var file in preservedFiles.OrderBy(f => f))
                    {
                        Log.Information("    - {File}", file);
                    }
                }
            }
            else
            {
                Log.Warning("  SQL directory not found: {Dir}", sqlDir);
            }

            // Step 3: Create fresh Initial migration
            Log.Information("");
            Log.Information("Step 3: Creating fresh Initial migration...");

            var migrationNamespace = $"Consilient.Data.Migrations.{contextShort}";
            var outputDir = contextShort;

            // Ensure migrations directory exists
            ((AbsolutePath)migrationsDir).CreateDirectory();

            // Create placeholder snapshot file so EF Core places migrations in correct location
            var newSnapshotFile = migrationsDir / $"{context}ModelSnapshot.cs";
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
            File.WriteAllText(newSnapshotFile, snapshotContent);

            // Run dotnet ef migrations add
            DotNet(
                $"ef migrations add Initial --context {context} --project \"{MigrationsProject}\" --startup-project \"{MigrationsProject}\" --output-dir {outputDir} --namespace {migrationNamespace} --verbose",
                workingDirectory: SourceDirectory);

            Log.Information("  Initial migration created successfully");

            // Step 4: Generate SQL script for the new Initial migration
            Log.Information("");
            Log.Information("Step 4: Generating SQL script for Initial migration...");

            // Find sequence number - get highest NON-MANUAL file number and add 1
            // This ensures the migration comes after other context migrations but manual files come after
            int nextNumber = 1;
            if (Directory.Exists(sqlDir))
            {
                var existingFiles = Directory.GetFiles(sqlDir, "*.sql")
                    .Select(Path.GetFileName)
                    .Where(f => Regex.IsMatch(f!, @"^\d{2}_") && !f!.Contains("_manual_"))
                    .OrderByDescending(f => f)
                    .ToList();

                if (existingFiles.Count > 0)
                {
                    var lastNumber = int.Parse(existingFiles[0]!.Substring(0, 2));
                    nextNumber = lastNumber + 1;
                }
            }

            var prefix = nextNumber.ToString("D2");
            var outputFile = sqlDir / $"{prefix}_{contextShortLower}_Initial.sql";

            // Ensure SQL directory exists
            ((AbsolutePath)sqlDir).CreateDirectory();

            // Generate script from "0" to the new Initial migration
            var newMigrations = Directory.GetFiles(migrationsDir, "*.cs")
                .Select(Path.GetFileName)
                .Where(f => Regex.IsMatch(f!, @"^\d{14}_.*\.cs$") && !f!.Contains(".Designer."))
                .OrderByDescending(f => f)
                .ToList();

            if (newMigrations.Count > 0)
            {
                var initialMigrationName = Path.GetFileNameWithoutExtension(newMigrations[0]!);

                DotNet(
                    $"ef migrations script 0 {initialMigrationName} --idempotent --context {context} --project \"{MigrationsProject}\" --startup-project \"{MigrationsProject}\" --output \"{outputFile}\"",
                    workingDirectory: SourceDirectory);

                // Prepend metadata header
                PrependMigrationScriptHeader(outputFile, context, initialMigrationName);

                Log.Information("  SQL script generated: {File}", outputFile);
            }
            else
            {
                Log.Warning("  No migrations found to generate script from");
            }

            // Step 5: Renumber manual files to come after the migration
            Log.Information("");
            Log.Information("Step 5: Ensuring manual files run after migrations...");

            var manualFiles = Directory.GetFiles(sqlDir, "*_manual_*.sql")
                .Select(f => new { Path = f, Name = Path.GetFileName(f) })
                .Where(f => Regex.IsMatch(f.Name!, @"^\d{2}_"))
                .OrderBy(f => f.Name)
                .ToList();

            int manualSequence = nextNumber + 1;
            foreach (var file in manualFiles)
            {
                var currentNumber = int.Parse(file.Name!.Substring(0, 2));
                if (currentNumber <= nextNumber)
                {
                    // Need to renumber this file to come after the migration
                    var newName = $"{manualSequence:D2}{file.Name!.Substring(2)}";
                    var newPath = Path.Combine(Path.GetDirectoryName(file.Path)!, newName);

                    Log.Information("  Renumbering: {Old} -> {New}", file.Name, newName);
                    File.Move(file.Path, newPath);
                    manualSequence++;
                }
            }

            // Summary
            Log.Information("");
            Log.Information("=== SQUASH COMPLETED ===");
            Log.Information("Next steps:");
            Log.Information("  1. Review the generated files");
            Log.Information("  2. Run '.\\build.ps1 ResetDatabase --force' to test with a fresh database");
            Log.Information("  3. Commit the changes");
        });

    /// <summary>
    /// Drop all database objects and rebuild from SQL scripts.
    /// Faster than ResetDatabase as it doesn't recreate the Docker container.
    /// </summary>
    /// <remarks>
    /// <para><b>Windows Usage:</b></para>
    /// <code>
    /// .\build.ps1 RebuildDatabase --force
    /// .\build.ps1 RebuildDatabase --force --backup
    /// </code>
    ///
    /// <para><b>Parameters:</b></para>
    /// <list type="bullet">
    ///   <item><c>--force</c> (required): Confirmation flag to prevent accidental execution</item>
    ///   <item><c>--backup</c> (optional): Create backup before dropping objects</item>
    ///   <item><c>--database</c> (optional): Target database name (default: consilient_main)</item>
    /// </list>
    ///
    /// <para><b>Differences from ResetDatabase:</b></para>
    /// <list type="bullet">
    ///   <item>RebuildDatabase: Drops objects then runs scripts (faster, no container restart)</item>
    ///   <item>ResetDatabase: Recreates Docker container with fresh image (slower, full reset)</item>
    /// </list>
    /// </remarks>
    Target RebuildDatabase => _ => _
        .Description("Drop all objects and rebuild database from SQL scripts (faster than ResetDatabase)")
        .Requires(() => Force)
        .Executes(() =>
        {
            Log.Warning("=== REBUILDING DATABASE {Database} ===", Database);

            // Step 1: Ensure container running (if Docker)
            if (ResolvedDbDocker)
            {
                Log.Information("Step 1: Ensuring database container is running...");
                EnsureContainerRunning();
                WaitForDatabaseHealthy();
            }
            else
            {
                Log.Information("Step 1: Using direct database connection (non-Docker mode)");
            }

            // Step 2: Backup (optional)
            if (Backup)
            {
                Log.Information("");
                Log.Information("Step 2: Creating backup...");
                BackupDatabase(Database);
            }
            else
            {
                Log.Information("");
                Log.Information("Step 2: Skipping backup (use --backup to enable)");
            }

            // Step 3: Drop all objects
            Log.Information("");
            Log.Information("Step 3: Dropping all database objects...");
            DropAllDatabaseObjects(Database);

            // Step 4: Run scripts
            Log.Information("");
            Log.Information("Step 4: Running SQL scripts...");

            var scriptsDir = DatabaseScriptsDir / $"{Database.Replace("_main", "").ToLower()}_main";
            if (!Directory.Exists(scriptsDir))
            {
                scriptsDir = DatabaseScriptsDir / Database.ToLower();
            }

            if (!Directory.Exists(scriptsDir))
            {
                throw new Exception($"Scripts directory not found: {scriptsDir}");
            }

            var scripts = GetOrderedScripts(scriptsDir);
            Log.Information("  Found {Count} script(s) to execute", scripts.Count);

            var failedScripts = new List<string>();
            foreach (var script in scripts)
            {
                if (!ExecuteSqlScript(script, Database))
                {
                    failedScripts.Add(Path.GetFileName(script));
                }
            }

            // Step 5: Verify
            Log.Information("");
            Log.Information("Step 5: Verifying database health...");
            VerifyRebuildDatabaseHealth(Database, failedScripts);

            Log.Information("");
            Log.Information("=== REBUILD COMPLETED ===");
        });

    // ============================================
    // DATABASE DOCUMENTATION TARGETS
    // ============================================

    [Parameter("Use Docker for SchemaSpy (default: true)")]
    readonly bool UseDocker = true;

    [Parameter("Environment name for docs")]
    readonly string Environment = "local";

    static AbsolutePath DatabaseDocsTemplateDir => RootDirectory / "build" / "database-docs" / "templates";
    static AbsolutePath DatabaseDocsOutputDir => DocsDirectory / "dbs";
    static AbsolutePath SchemaSpyDriversDir => RootDirectory / "src" / ".docker" / "schemaspy-drivers";

    const string MssqlJdbcDriverVersion = "12.6.1.jre11";
    const string MssqlJdbcDriverFileName = $"mssql-jdbc-{MssqlJdbcDriverVersion}.jar";
    const string MssqlJdbcDriverUrl = $"https://repo1.maven.org/maven2/com/microsoft/sqlserver/mssql-jdbc/12.6.1.jre11/mssql-jdbc-{MssqlJdbcDriverVersion}.jar";

    Target GenerateDatabaseDocs => _ => _
        .Description("Generate SchemaSpy database documentation")
        .Executes(() =>
        {
            Log.Information("Generating database documentation...");

            // Ensure JDBC driver is available for SchemaSpy
            EnsureJdbcDriver();

            // Ensure output directory exists
            DatabaseDocsOutputDir.CreateDirectory();

            // Get databases to document - all schemas
            var databases = new[] { ("consilient_main", "consilient_main", new[] { "billing", "clinical", "compensation", "identity", "staging" }) };

            try
            {
                // Handle container lifecycle
                if (ResolvedDbDocker && ResolvedDbAutoStart)
                {
                    EnsureContainerRunning();
                }
                else if (ResolvedDbDocker)
                {
                    CheckContainerHealth();
                }

                foreach (var (dbName, actualDbName, schemas) in databases)
                {
                    var dbOutputDir = DatabaseDocsOutputDir / dbName.ToLower();
                    dbOutputDir.CreateDirectory();

                    foreach (var schema in schemas)
                    {
                        var schemaOutputDir = dbOutputDir / schema.ToLower();
                        schemaOutputDir.CreateDirectory();

                        Log.Information("Generating docs for {Database}.{Schema}...", actualDbName, schema);

                        if (ResolvedDbDocker)
                        {
                            RunSchemaSpyViaDockerCompose(actualDbName, schema, schemaOutputDir);
                        }
                        else if (UseDocker)
                        {
                            RunSchemaSpyViaDocker(actualDbName, schema, schemaOutputDir);
                        }
                        else
                        {
                            RunSchemaSpyDirect(actualDbName, schema, schemaOutputDir);
                        }
                    }

                    // Generate per-database index page
                    GenerateDatabaseIndexPage(dbName, actualDbName, schemas, dbOutputDir);
                }

                // Generate main index page
                GenerateMainIndexPage(databases);

                // Create root redirect
                var rootRedirect = DocsDirectory / "index.html";
                if (!rootRedirect.FileExists() || File.ReadAllText(rootRedirect).Contains("Redirecting to"))
                {
                    File.WriteAllText(rootRedirect, @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta http-equiv=""refresh"" content=""0; url=dbs/"">
    <title>Redirecting...</title>
</head>
<body>
    <p>Redirecting to <a href=""dbs/"">database documentation</a>...</p>
</body>
</html>");
                }

                Log.Information("Database documentation generated at {Path}", DatabaseDocsOutputDir);
            }
            finally
            {
                // Stop container if we started it
                if (ResolvedDbDocker && ResolvedDbAutoStart)
                {
                    StopContainerIfStartedByBuild();
                }
            }
        });

    void RunSchemaSpyViaDockerCompose(string database, string schema, AbsolutePath outputDir)
    {
        Log.Information("Running SchemaSpy via docker-compose for {Database}.{Schema}...", database, schema);

        var composeDir = ResolvedDbComposeFile.Parent;
        var composeFileName = ResolvedDbComposeFile.Name;

        // Calculate relative output path from compose file location
        var relativeOutputPath = outputDir.ToString().Replace(RootDirectory.ToString(), "").TrimStart('\\', '/');
        var dockerOutputPath = $"/output/{database}/{schema}";

        // Set environment variables for this run
        var envVars = new Dictionary<string, string>
        {
            ["SCHEMASPY_SCHEMA"] = schema,
            ["SCHEMASPY_OUTPUT"] = dockerOutputPath,
            ["SA_PASSWORD"] = ResolvedDbPassword
        };

        // Use --no-deps since we already verified db is healthy (avoids container name conflicts)
        var args = $"compose -f \"{composeFileName}\" --profile tools run --rm --no-deps schemaspy";

        Log.Debug("Running: docker {Args} with SCHEMASPY_SCHEMA={Schema}, SCHEMASPY_OUTPUT={Output}",
            args, schema, dockerOutputPath);

        RunDockerComposeWithEnv(args, composeDir, envVars);

        Log.Information("SchemaSpy completed for {Database}.{Schema}", database, schema);
    }

    void RunSchemaSpyViaDocker(string database, string schema, AbsolutePath outputDir)
    {
        Log.Information("Running SchemaSpy via Docker for {Database}.{Schema}...", database, schema);

        // Note: On Windows, we need to use the host.docker.internal to reach the host's localhost
        var hostAddress = OperatingSystem.IsWindows() ? "host.docker.internal" : "localhost";

        var args = $"run --rm " +
            $"-v \"{outputDir}:/output\" " +
            $"schemaspy/schemaspy:latest " +
            $"-t mssql17 " +
            $"-host {hostAddress} -port {ResolvedDbPort} " +
            $"-db {database} " +
            $"-u {ResolvedDbUser} -p {ResolvedDbPassword} " +
            $"-s {schema} " +
            $"-connprops \"encrypt=false;trustServerCertificate=true\" " +
            $"-norows -vizjs -imageformat svg";

        var process = ProcessTasks.StartProcess("docker", args, workingDirectory: RootDirectory);
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Log.Warning("SchemaSpy failed for {Database}.{Schema}. Check if database is running and accessible.", database, schema);
        }
    }

    void RunSchemaSpyDirect(string database, string schema, AbsolutePath outputDir)
    {
        Log.Information("Running SchemaSpy directly for {Database}.{Schema}...", database, schema);

        var schemaSpyJar = RootDirectory / "build" / "tools" / "schemaspy.jar";
        var jdbcDriver = RootDirectory / "build" / "tools" / "mssql-jdbc.jar";

        if (!schemaSpyJar.FileExists())
        {
            Log.Error("SchemaSpy JAR not found at {Path}. Download it or use --use-docker", schemaSpyJar);
            throw new Exception($"SchemaSpy JAR not found: {schemaSpyJar}");
        }

        var args = $"-jar \"{schemaSpyJar}\" " +
            $"-t mssql17 " +
            $"-dp \"{jdbcDriver.Parent}\" " +
            $"-host {ResolvedDbHost} -port {ResolvedDbPort} " +
            $"-db {database} " +
            $"-u {ResolvedDbUser} -p {ResolvedDbPassword} " +
            $"-s {schema} " +
            $"-o \"{outputDir}\" " +
            $"-connprops \"encrypt=false;trustServerCertificate=true\" " +
            $"-norows -vizjs -imageformat svg";

        var process = ProcessTasks.StartProcess("java", args, workingDirectory: RootDirectory);
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Log.Warning("SchemaSpy failed for {Database}.{Schema}. Ensure Java 17+ is installed.", database, schema);
        }
    }

    void GenerateMainIndexPage((string dbName, string actualDbName, string[] schemas)[] databases)
    {
        var templatePath = DatabaseDocsTemplateDir / "index.template.html";
        if (!templatePath.FileExists())
        {
            Log.Warning("Index template not found at {Path}, skipping index generation", templatePath);
            return;
        }

        var template = File.ReadAllText(templatePath);

        // Build database cards HTML
        var databaseCards = new System.Text.StringBuilder();
        foreach (var (dbName, actualDbName, schemas) in databases)
        {
            var schemaList = string.Join("", schemas.Select(s => $"<li>{HtmlEncode(s)}</li>"));
            databaseCards.AppendLine($@"<div class=""database-card"">
      <h2>🗄️ {HtmlEncode(dbName)}</h2>
      <p><strong>Physical Database:</strong> {HtmlEncode(actualDbName)}</p>
      <p><strong>Schemas ({schemas.Length}):</strong></p>
      <ul>{schemaList}</ul>
      <a href=""./{dbName.ToLower()}/index.html"" class=""btn"">View Documentation →</a>
    </div>");
        }

        var content = template
            .Replace("{{CURRENT_DATE}}", HtmlEncode(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")))
            .Replace("{{DATABASE_COUNT}}", databases.Length.ToString())
            .Replace("{{ENVIRONMENT}}", HtmlEncode(Environment))
            .Replace("{{DATABASE_CARDS}}", databaseCards.ToString());

        var outputPath = DatabaseDocsOutputDir / "index.html";
        File.WriteAllText(outputPath, content);
        Log.Information("Generated main index at {Path}", outputPath);
    }

    void GenerateDatabaseIndexPage(string dbName, string actualDbName, string[] schemas, AbsolutePath outputDir)
    {
        var templatePath = DatabaseDocsTemplateDir / "database.template.html";
        if (!templatePath.FileExists())
        {
            Log.Warning("Database template not found at {Path}, skipping database index", templatePath);
            return;
        }

        var template = File.ReadAllText(templatePath);

        // Build schema cards HTML
        var schemaCards = new System.Text.StringBuilder();
        foreach (var schema in schemas)
        {
            schemaCards.AppendLine($@"<div class=""schema-card"">
          <h2>📋 {HtmlEncode(schema)}</h2>
          <p>View tables, relationships, and constraints for the <strong>{HtmlEncode(schema)}</strong> schema.</p>
          <a href=""./{schema.ToLower()}/index.html"" class=""btn"">View Schema →</a>
        </div>");
        }

        var content = template
            .Replace("{{DB_NAME}}", HtmlEncode(dbName))
            .Replace("{{ACTUAL_DB_NAME}}", HtmlEncode(actualDbName))
            .Replace("{{SCHEMA_COUNT}}", schemas.Length.ToString())
            .Replace("{{ENVIRONMENT}}", HtmlEncode(Environment))
            .Replace("{{CURRENT_DATE}}", HtmlEncode(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")))
            .Replace("{{SCHEMA_CARDS}}", schemaCards.ToString());

        var outputPath = outputDir / "index.html";
        File.WriteAllText(outputPath, content);
        Log.Information("Generated database index at {Path}", outputPath);
    }

    static string HtmlEncode(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return input
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    /// <summary>
    /// Prepends a metadata header to a generated migration SQL script.
    /// This header identifies the file as auto-generated and records source information.
    /// Also includes SET options required for SQL Server indexed views and computed columns.
    /// </summary>
    void PrependMigrationScriptHeader(AbsolutePath filePath, string context, string migrationName)
    {
        var generatedSql = File.ReadAllText(filePath);
        var header = $@"-- ============================================
-- EF Core Migration Script (Auto-Generated)
-- ============================================
-- Context:     {context}
-- Migration:   {migrationName}
-- Generated:   {DateTime.UtcNow:O}
-- ============================================
-- WARNING: This file is auto-generated.
-- It will be deleted by SquashMigrations.
-- Do not manually edit unless necessary.
-- ============================================

-- Required SET options for SQL Server indexed views and computed columns
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

";
        File.WriteAllText(filePath, header + generatedSql);
    }

    /// <summary>
    /// Checks if a SQL file is an auto-generated migration script for a specific context.
    /// Detection is based on the metadata header added by GenerateMigrationScript.
    /// </summary>
    bool IsAutoGeneratedMigrationScript(string filePath, string expectedContext)
    {
        try
        {
            var firstLines = File.ReadLines(filePath).Take(10).ToList();
            return firstLines.Any(line => line.Contains("EF Core Migration Script (Auto-Generated)")) &&
                   firstLines.Any(line => line.Contains($"Context:     {expectedContext}"));
        }
        catch
        {
            return false;
        }
    }

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
            process.WaitForExit();

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

    bool IsContainerRunning(string containerName)
    {
        var process = ProcessTasks.StartProcess(
            "docker",
            $"inspect {containerName} --format=\"{{{{.State.Running}}}}\"",
            workingDirectory: RootDirectory);
        process.WaitForExit();

        return process.ExitCode == 0 &&
            string.Join("", process.Output.Select(o => o.Text)).Contains("true");
    }

    void EnsureContainerRunning()
    {
        if (!ResolvedDbDocker)
        {
            Log.Debug("Docker mode disabled, skipping container check");
            return;
        }

        if (IsContainerRunning(ResolvedDbContainerName))
        {
            Log.Information("Database container {Container} is already running", ResolvedDbContainerName);
            return;
        }

        Log.Information("Starting database container via docker-compose...");
        var composeDir = ResolvedDbComposeFile.Parent;
        var composeFileName = ResolvedDbComposeFile.Name;

        var process = ProcessTasks.StartProcess(
            "docker",
            $"compose -f \"{composeFileName}\" up -d {ResolvedDbServiceName}",
            workingDirectory: composeDir);
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Failed to start database container. Exit code: {process.ExitCode}");
        }

        _containerStartedByBuild = true;
        Log.Information("Database container started");

        // Wait for healthy status
        WaitForDatabaseHealthy(retries: 15, intervalSeconds: 5);
    }

    void StopContainerIfStartedByBuild()
    {
        if (!_containerStartedByBuild)
        {
            Log.Debug("Container was not started by build, skipping stop");
            return;
        }

        Log.Information("Stopping database container (was started by build)...");
        var composeDir = ResolvedDbComposeFile.Parent;
        var composeFileName = ResolvedDbComposeFile.Name;

        var process = ProcessTasks.StartProcess(
            "docker",
            $"compose -f \"{composeFileName}\" stop {ResolvedDbServiceName}",
            workingDirectory: composeDir);
        process.WaitForExit();

        _containerStartedByBuild = false;
        Log.Information("Database container stopped");
    }

    void CheckContainerHealth()
    {
        if (!ResolvedDbDocker)
        {
            Log.Debug("Docker mode disabled, skipping health check");
            return;
        }

        if (!IsContainerRunning(ResolvedDbContainerName))
        {
            throw new Exception($"Database container '{ResolvedDbContainerName}' is not running. Start it with: docker compose up -d {ResolvedDbServiceName}");
        }

        Log.Information("Checking database container health...");
        var process = ProcessTasks.StartProcess(
            "docker",
            $"inspect {ResolvedDbContainerName} --format=\"{{{{.State.Health.Status}}}}\"",
            workingDirectory: RootDirectory);
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Failed to check container health for '{ResolvedDbContainerName}'");
        }

        var status = string.Join("", process.Output.Select(o => o.Text)).Trim().Trim('"');
        if (status != "healthy")
        {
            throw new Exception($"Database container is not healthy. Current status: {status}");
        }

        Log.Information("Database container is healthy");
    }

    void RunDockerComposeWithEnv(string arguments, AbsolutePath workingDirectory, Dictionary<string, string>? envVars = null)
    {
        Log.Debug("Running docker {Arguments}", arguments);

        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        // Add environment variables
        if (envVars != null)
        {
            foreach (var (key, value) in envVars)
            {
                startInfo.EnvironmentVariables[key] = value;
            }
        }

        using var process = new Process { StartInfo = startInfo };

        var output = new List<string>();
        var errors = new List<string>();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                output.Add(e.Data);
                Log.Debug(e.Data);
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                errors.Add(e.Data);
                Log.Debug(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var errorOutput = string.Join("\n", errors);
            Log.Error("Docker command failed: {Error}", errorOutput);
            throw new Exception($"Docker command failed with exit code {process.ExitCode}");
        }
    }

    void EnsureJdbcDriver()
    {
        var driverPath = SchemaSpyDriversDir / MssqlJdbcDriverFileName;

        if (driverPath.FileExists())
        {
            Log.Debug("JDBC driver already exists at {Path}", driverPath);
            return;
        }

        Log.Information("Downloading MSSQL JDBC driver {Version}...", MssqlJdbcDriverVersion);
        SchemaSpyDriversDir.CreateDirectory();

        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(5);

        try
        {
            var response = httpClient.GetAsync(MssqlJdbcDriverUrl).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            using var fileStream = File.Create(driverPath);
            response.Content.CopyToAsync(fileStream).GetAwaiter().GetResult();

            Log.Information("JDBC driver downloaded to {Path}", driverPath);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to download JDBC driver from {MssqlJdbcDriverUrl}: {ex.Message}", ex);
        }
    }

    // ============================================
    // REBUILD DATABASE HELPER METHODS
    // ============================================

    /// <summary>
    /// Drops all database objects in dependency order:
    /// Foreign keys → Views → Procedures → Functions → Tables → Types → Schemas
    /// </summary>
    void DropAllDatabaseObjects(string database)
    {
        var dropScript = @"
SET NOCOUNT ON;
DECLARE @sql NVARCHAR(MAX);

-- Drop all foreign keys
SET @sql = N'';
SELECT @sql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
    + ' DROP CONSTRAINT ' + QUOTENAME(f.name) + ';' + CHAR(13)
FROM sys.foreign_keys f
JOIN sys.tables t ON f.parent_object_id = t.object_id
JOIN sys.schemas s ON t.schema_id = s.schema_id;
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
PRINT 'Dropped foreign keys';

-- Drop all views
SET @sql = N'';
SELECT @sql += 'DROP VIEW ' + QUOTENAME(s.name) + '.' + QUOTENAME(v.name) + ';' + CHAR(13)
FROM sys.views v
JOIN sys.schemas s ON v.schema_id = s.schema_id
WHERE s.name NOT IN ('sys', 'INFORMATION_SCHEMA');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
PRINT 'Dropped views';

-- Drop all stored procedures
SET @sql = N'';
SELECT @sql += 'DROP PROCEDURE ' + QUOTENAME(s.name) + '.' + QUOTENAME(p.name) + ';' + CHAR(13)
FROM sys.procedures p
JOIN sys.schemas s ON p.schema_id = s.schema_id
WHERE s.name NOT IN ('sys', 'INFORMATION_SCHEMA');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
PRINT 'Dropped stored procedures';

-- Drop all functions
SET @sql = N'';
SELECT @sql += 'DROP FUNCTION ' + QUOTENAME(s.name) + '.' + QUOTENAME(o.name) + ';' + CHAR(13)
FROM sys.objects o
JOIN sys.schemas s ON o.schema_id = s.schema_id
WHERE o.type IN ('FN','IF','TF') AND s.name NOT IN ('sys', 'INFORMATION_SCHEMA');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
PRINT 'Dropped functions';

-- Drop all tables
SET @sql = N'';
SELECT @sql += 'DROP TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ';' + CHAR(13)
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name NOT IN ('sys', 'INFORMATION_SCHEMA');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
PRINT 'Dropped tables';

-- Drop all user-defined types
SET @sql = N'';
SELECT @sql += 'DROP TYPE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ';' + CHAR(13)
FROM sys.types t
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE t.is_user_defined = 1;
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
PRINT 'Dropped user-defined types';

-- Drop all non-system schemas (except dbo, guest, sys, INFORMATION_SCHEMA)
SET @sql = N'';
SELECT @sql += 'DROP SCHEMA ' + QUOTENAME(name) + ';' + CHAR(13)
FROM sys.schemas
WHERE name NOT IN ('dbo','guest','sys','INFORMATION_SCHEMA','db_owner','db_accessadmin','db_securityadmin','db_ddladmin','db_backupoperator','db_datareader','db_datawriter','db_denydatareader','db_denydatawriter')
AND schema_id > 4;
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
PRINT 'Dropped schemas';

PRINT 'All database objects dropped successfully';
";

        ExecuteSqlCommand(database, dropScript);
        Log.Information("  All objects dropped from {Database}", database);
    }

    /// <summary>
    /// Executes a SQL command against the specified database.
    /// Uses Docker exec if running in Docker mode, otherwise direct sqlcmd.
    /// </summary>
    void ExecuteSqlCommand(string database, string sqlCommand)
    {
        // Write SQL to temp file for execution
        var tempSqlFile = Path.GetTempFileName() + ".sql";
        File.WriteAllText(tempSqlFile, sqlCommand);

        try
        {
            if (ResolvedDbDocker)
            {
                // Copy script to container
                var containerPath = "/tmp/command.sql";
                var copyProcess = ProcessTasks.StartProcess(
                    "docker",
                    $"cp \"{tempSqlFile}\" {ResolvedDbContainerName}:{containerPath}",
                    workingDirectory: RootDirectory);
                copyProcess.WaitForExit();

                if (copyProcess.ExitCode != 0)
                {
                    throw new Exception("Failed to copy SQL file to container");
                }

                // Execute via sqlcmd inside container using $SA_PASSWORD env var
                var execProcess = ProcessTasks.StartProcess(
                    "docker",
                    $"exec {ResolvedDbContainerName} bash -c \"/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P \\\"$SA_PASSWORD\\\" -d {database} -C -i {containerPath}\"",
                    workingDirectory: RootDirectory);
                execProcess.WaitForExit();

                // Clean up
                ProcessTasks.StartProcess("docker", $"exec {ResolvedDbContainerName} rm {containerPath}", workingDirectory: RootDirectory).WaitForExit();

                if (execProcess.ExitCode != 0)
                {
                    var output = string.Join("\n", execProcess.Output.Select(o => o.Text));
                    throw new Exception($"SQL command failed: {output}");
                }
            }
            else
            {
                // Direct sqlcmd execution
                var args = $"-S {ResolvedDbHost},{ResolvedDbPort} -U {ResolvedDbUser} -P \"{ResolvedDbPassword}\" -d {database} -i \"{tempSqlFile}\"";
                var process = ProcessTasks.StartProcess("sqlcmd", args, workingDirectory: RootDirectory);
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    var output = string.Join("\n", process.Output.Select(o => o.Text));
                    throw new Exception($"SQL command failed: {output}");
                }
            }
        }
        finally
        {
            // Clean up temp file
            if (File.Exists(tempSqlFile))
                File.Delete(tempSqlFile);
        }
    }

    /// <summary>
    /// Executes a SQL script file against the specified database.
    /// Returns true if successful, false if failed.
    /// </summary>
    bool ExecuteSqlScript(string scriptPath, string database)
    {
        var scriptName = Path.GetFileName(scriptPath);
        Log.Information("  Running: {Script}", scriptName);

        try
        {
            if (ResolvedDbDocker)
            {
                // Copy script to container
                var containerPath = $"/tmp/{scriptName}";
                var copyProcess = ProcessTasks.StartProcess(
                    "docker",
                    $"cp \"{scriptPath}\" {ResolvedDbContainerName}:{containerPath}",
                    workingDirectory: RootDirectory);
                copyProcess.WaitForExit();

                if (copyProcess.ExitCode != 0)
                {
                    Log.Error("    FAILED to copy script to container: {Script}", scriptName);
                    return false;
                }

                // Execute via sqlcmd inside container using $SA_PASSWORD env var
                var execProcess = ProcessTasks.StartProcess(
                    "docker",
                    $"exec {ResolvedDbContainerName} bash -c \"/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P \\\"$SA_PASSWORD\\\" -d {database} -C -i {containerPath}\"",
                    workingDirectory: RootDirectory);
                execProcess.WaitForExit();

                // Clean up
                ProcessTasks.StartProcess("docker", $"exec {ResolvedDbContainerName} rm {containerPath}", workingDirectory: RootDirectory).WaitForExit();

                if (execProcess.ExitCode != 0)
                {
                    var output = string.Join("\n", execProcess.Output.Select(o => o.Text));
                    Log.Error("    FAILED: {Script}", scriptName);
                    Log.Error("    Output: {Output}", output);
                    return false;
                }
            }
            else
            {
                // Direct sqlcmd execution
                var args = $"-S {ResolvedDbHost},{ResolvedDbPort} -U {ResolvedDbUser} -P \"{ResolvedDbPassword}\" -d {database} -i \"{scriptPath}\"";
                var process = ProcessTasks.StartProcess("sqlcmd", args, workingDirectory: RootDirectory);
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    var output = string.Join("\n", process.Output.Select(o => o.Text));
                    Log.Error("    FAILED: {Script}", scriptName);
                    Log.Error("    Output: {Output}", output);
                    return false;
                }
            }

            Log.Information("    OK: {Script}", scriptName);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error("    EXCEPTION: {Script} - {Message}", scriptName, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Gets SQL scripts from the specified directory, ordered by filename.
    /// seed.sql is always placed last.
    /// </summary>
    IReadOnlyList<string> GetOrderedScripts(AbsolutePath scriptsDir)
    {
        var allScripts = Directory.GetFiles(scriptsDir, "*.sql")
            .Select(f => new { Path = f, Name = Path.GetFileName(f)! })
            .OrderBy(f => f.Name)
            .ToList();

        // Separate numbered scripts from seed.sql
        var numberedScripts = allScripts
            .Where(f => Regex.IsMatch(f.Name, @"^\d{2}_"))
            .Select(f => f.Path)
            .ToList();

        // seed.sql always runs last
        var seedScript = allScripts.FirstOrDefault(f => f.Name.Equals("seed.sql", StringComparison.OrdinalIgnoreCase));
        if (seedScript != null)
        {
            numberedScripts.Add(seedScript.Path);
        }

        return numberedScripts;
    }

    /// <summary>
    /// Creates a backup of the specified database.
    /// </summary>
    void BackupDatabase(string database)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"{database}_{timestamp}.bak";

        if (ResolvedDbDocker)
        {
            // Create backup inside container
            var containerBackupPath = $"/tmp/{backupFileName}";
            var backupSql = $"BACKUP DATABASE [{database}] TO DISK = N'{containerBackupPath}' WITH FORMAT, INIT, NAME = N'{database}-Full Backup';";

            Log.Information("  Creating backup in container: {Path}", containerBackupPath);
            ExecuteSqlCommand(database, backupSql);

            // Copy backup from container to host temp directory
            var hostBackupPath = Path.Combine(Path.GetTempPath(), backupFileName);
            var copyProcess = ProcessTasks.StartProcess(
                "docker",
                $"cp {ResolvedDbContainerName}:{containerBackupPath} \"{hostBackupPath}\"",
                workingDirectory: RootDirectory);
            copyProcess.WaitForExit();

            if (copyProcess.ExitCode == 0)
            {
                Log.Information("  Backup saved to: {Path}", hostBackupPath);

                // Clean up container backup
                ProcessTasks.StartProcess("docker", $"exec {ResolvedDbContainerName} rm {containerBackupPath}", workingDirectory: RootDirectory).WaitForExit();
            }
            else
            {
                Log.Warning("  Backup created in container but failed to copy to host");
                Log.Warning("  Container path: {Path}", containerBackupPath);
            }
        }
        else
        {
            // Direct backup to host temp directory
            var hostBackupPath = Path.Combine(Path.GetTempPath(), backupFileName);
            var backupSql = $"BACKUP DATABASE [{database}] TO DISK = N'{hostBackupPath}' WITH FORMAT, INIT, NAME = N'{database}-Full Backup';";

            Log.Information("  Creating backup: {Path}", hostBackupPath);
            ExecuteSqlCommand(database, backupSql);
            Log.Information("  Backup saved to: {Path}", hostBackupPath);
        }
    }

    /// <summary>
    /// Verifies database health after rebuild by counting tables and checking for failed scripts.
    /// </summary>
    void VerifyRebuildDatabaseHealth(string database, List<string> failedScripts)
    {
        // Count tables using temp file to avoid quoting issues
        var countSql = $"SET NOCOUNT ON; SELECT COUNT(*) FROM {database}.INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
        int tableCount = 0;

        var tempSqlFile = Path.GetTempFileName() + ".sql";
        File.WriteAllText(tempSqlFile, countSql);

        try
        {
            if (ResolvedDbDocker)
            {
                // Copy script to container
                var containerPath = "/tmp/count_tables.sql";
                var copyProcess = ProcessTasks.StartProcess(
                    "docker",
                    $"cp \"{tempSqlFile}\" {ResolvedDbContainerName}:{containerPath}",
                    workingDirectory: RootDirectory);
                copyProcess.WaitForExit();

                var process = ProcessTasks.StartProcess(
                    "docker",
                    $"exec {ResolvedDbContainerName} bash -c \"/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P \\\"$SA_PASSWORD\\\" -C -h -1 -i {containerPath}\"",
                    workingDirectory: RootDirectory);
                process.WaitForExit();

                // Clean up
                ProcessTasks.StartProcess("docker", $"exec {ResolvedDbContainerName} rm -f {containerPath}", workingDirectory: RootDirectory).WaitForExit();

                if (process.ExitCode == 0)
                {
                    var output = string.Join("", process.Output.Select(o => o.Text)).Trim();
                    int.TryParse(output, out tableCount);
                }
            }
            else
            {
                var args = $"-S {ResolvedDbHost},{ResolvedDbPort} -U {ResolvedDbUser} -P \"{ResolvedDbPassword}\" -h -1 -i \"{tempSqlFile}\"";
                var process = ProcessTasks.StartProcess("sqlcmd", args, workingDirectory: RootDirectory);
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    var output = string.Join("", process.Output.Select(o => o.Text)).Trim();
                    int.TryParse(output, out tableCount);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning("  Failed to count tables: {Message}", ex.Message);
        }
        finally
        {
            if (File.Exists(tempSqlFile))
                File.Delete(tempSqlFile);
        }

        // Report results
        if (failedScripts.Count > 0)
        {
            Log.Error("  {Count} script(s) FAILED:", failedScripts.Count);
            foreach (var script in failedScripts)
            {
                Log.Error("    - {Script}", script);
            }
        }

        if (tableCount > 0)
        {
            if (tableCount < 10)
            {
                Log.Warning("  Only {TableCount} tables created - this may indicate script failures", tableCount);
            }
            else
            {
                Log.Information("  SUCCESS: {TableCount} tables created in {Database}", tableCount, database);
            }
        }
        else
        {
            Log.Warning("  Could not verify table count");
        }

        if (failedScripts.Count > 0)
        {
            throw new Exception($"Database rebuild completed with {failedScripts.Count} failed script(s)");
        }
    }
}
