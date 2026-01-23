using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Serilog;

/// <summary>
/// Docker container management targets.
/// Handles: Building, starting, stopping, and restarting Docker services.
/// </summary>
partial class Build
{
    // ============================================
    // DOCKER TARGETS
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

    static void RunDocker(string arguments, AbsolutePath workingDirectory)
    {
        Log.Debug("Running docker {Arguments}", arguments);
        var process = ProcessTasks.StartProcess(
            "docker",
            arguments,
            workingDirectory: workingDirectory);
        process.AssertZeroExitCode();
    }
}
