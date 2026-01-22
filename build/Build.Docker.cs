using System;
using System.Linq;
using System.Threading;
using Nuke.Common;
using Nuke.Common.Tooling;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    // Database container configuration
    const string DatabaseContainerName = "consilient.dbs.container";
    const string DatabaseConnectionString = "Server=localhost,1434;Database=consilient_main;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";

    // ============================================
    // DATABASE TARGETS (Low Complexity)
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

    // ============================================
    // DATABASE TARGETS (Medium Complexity)
    // ============================================

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

    // ============================================
    // DOCKER TARGETS (Medium Complexity)
    // ============================================

    Target DockerBuild => _ => _
        .Description("Build all Docker images")
        .Executes(() =>
        {
            Log.Information("Building Docker images...");
            RunDocker($"compose -f \"{DockerComposeFile}\" build", DockerDirectory);
            Log.Information("Docker images built successfully");
        });

    Target DockerUp => _ => _
        .Description("Start all Docker services")
        .Executes(() =>
        {
            Log.Information("Starting Docker services...");
            RunDocker($"compose -f \"{DockerComposeFile}\" up -d", DockerDirectory);
            Log.Information("Docker services started");
        });

    Target DockerDown => _ => _
        .Description("Stop all Docker services")
        .Executes(() =>
        {
            Log.Information("Stopping Docker services...");
            RunDocker($"compose -f \"{DockerComposeFile}\" down", DockerDirectory);
            Log.Information("Docker services stopped");
        });

    Target DockerRestart => _ => _
        .DependsOn(DockerDown)
        .Description("Restart all Docker services")
        .Executes(() =>
        {
            Log.Information("Starting Docker services...");
            RunDocker($"compose -f \"{DockerComposeFile}\" up -d", DockerDirectory);
            Log.Information("Docker services restarted");
        });

    // ============================================
    // HELPER METHODS
    // ============================================

    static void RunDocker(string arguments, Nuke.Common.IO.AbsolutePath workingDirectory)
    {
        Log.Debug("Running docker {Arguments}", arguments);
        var process = ProcessTasks.StartProcess(
            "docker",
            arguments,
            workingDirectory: workingDirectory);
        process.AssertZeroExitCode();
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
