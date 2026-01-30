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

    Target DockerNuclearReset => _ => _
        .Description("Complete Docker cleanup: remove all Consilient containers, images, volumes, and networks")
        .Requires(() => Force)
        .Executes(() =>
        {
            Log.Information("Starting Nuclear Docker Reset...");

            // Step 1: Stop and remove containers via docker-compose
            Log.Information("Stopping and removing containers via docker-compose...");
            try
            {
                RunDocker($"compose -f \"{DockerComposeFile}\" down --volumes --remove-orphans", DockerDirectory);
            }
            catch (Exception ex)
            {
                Log.Warning("docker-compose down failed (may be normal if no containers): {Message}", ex.Message);
            }

            // Step 2: Find all images with "consilient" in their name
            Log.Information("Searching for Consilient Docker images...");
            var imageIdsOutput = GetDockerOutput("images --filter \"reference=*consilient*\" -q", DockerDirectory);
            var imageIds = imageIdsOutput
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToList();

            if (imageIds.Count > 0)
            {
                // Step 3: Find and remove containers using these images
                Log.Information("Found {Count} Consilient images, checking for lingering containers...", imageIds.Count);
                var containersToRemove = new List<string>();

                foreach (var imageId in imageIds)
                {
                    var containerIds = GetDockerOutput($"ps -a --filter \"ancestor={imageId}\" -q", DockerDirectory);
                    containersToRemove.AddRange(containerIds
                        .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                        .Where(id => !string.IsNullOrWhiteSpace(id)));
                }

                if (containersToRemove.Count > 0)
                {
                    Log.Information("Stopping and removing {Count} lingering containers...", containersToRemove.Count);
                    foreach (var containerId in containersToRemove.Distinct())
                    {
                        try
                        {
                            RunDocker($"stop {containerId}", DockerDirectory);
                            RunDocker($"rm {containerId}", DockerDirectory);
                        }
                        catch (Exception ex)
                        {
                            Log.Warning("Failed to remove container {Id}: {Message}", containerId, ex.Message);
                        }
                    }
                }
                else
                {
                    Log.Information("No lingering containers found.");
                }

                // Step 4: Remove the images
                Log.Information("Removing Consilient Docker images...");
                foreach (var imageId in imageIds)
                {
                    try
                    {
                        RunDocker($"rmi -f {imageId}", DockerDirectory);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning("Failed to remove image {Id}: {Message}", imageId, ex.Message);
                    }
                }
            }
            else
            {
                Log.Information("No Consilient images found.");
            }

            // Step 5: Prune volumes
            Log.Information("Pruning unused volumes...");
            try
            {
                RunDocker("volume prune -f", DockerDirectory);
            }
            catch (Exception ex)
            {
                Log.Warning("Volume prune failed: {Message}", ex.Message);
            }

            // Step 6: Prune networks
            Log.Information("Pruning unused networks...");
            try
            {
                RunDocker("network prune -f", DockerDirectory);
            }
            catch (Exception ex)
            {
                Log.Warning("Network prune failed: {Message}", ex.Message);
            }

            Log.Information("Nuclear Docker Reset complete!");
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

    static string GetDockerOutput(string arguments, AbsolutePath workingDirectory)
    {
        Log.Debug("Running docker {Arguments}", arguments);
        var process = ProcessTasks.StartProcess(
            "docker",
            arguments,
            workingDirectory: workingDirectory);
        process.AssertZeroExitCode();
        return string.Join(System.Environment.NewLine, process.Output.Select(o => o.Text));
    }
}
