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
    static AbsolutePath EnvLocalFile => RootDirectory / "scripts" / ".env.local";

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
            var databases = new[] { ("consilient_main", "consilient_main", new[] { "clinical", "compensation", "identity", "staging" }) };

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
}
