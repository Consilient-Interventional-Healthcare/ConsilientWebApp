using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Serilog;

/// <summary>
/// Wrapper for Swashbuckle CLI (swagger tofile) to generate OpenAPI documentation.
/// Uses assembly reflection - no application startup required.
/// </summary>
public static class SwaggerCliTool
{
    /// <summary>
    /// Generates an OpenAPI document from a compiled API assembly.
    /// </summary>
    /// <param name="dllPath">Path to the compiled API DLL</param>
    /// <param name="outputPath">Path where the OpenAPI JSON will be written</param>
    /// <param name="documentName">OpenAPI document name (default: "v1")</param>
    /// <param name="prettyPrint">Format JSON with indentation (default: true)</param>
    public static void GenerateDocument(
        AbsolutePath dllPath,
        AbsolutePath outputPath,
        string documentName = "v1",
        bool prettyPrint = true)
    {
        if (!File.Exists(dllPath))
        {
            throw new FileNotFoundException($"API DLL not found: {dllPath}. Ensure project is built.");
        }

        // Ensure output directory exists
        outputPath.Parent.CreateDirectory();

        // Set environment for Swashbuckle reflection
        var previousAspNetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var previousDotNetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

        try
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");

            Log.Debug("Set ASPNETCORE_ENVIRONMENT=Development for assembly reflection");

            // Generate to temp file first
            var tempFile = Path.GetTempFileName();

            try
            {
                // Run swagger tofile via dotnet tool
                var arguments = $"tool run swagger tofile --output \"{tempFile}\" \"{dllPath}\" {documentName}";

                Log.Information("Generating OpenAPI documentation from assembly...");
                Log.Debug("Running: dotnet {Arguments}", arguments);

                var process = ProcessTasks.StartProcess(
                    "dotnet",
                    arguments,
                    workingDirectory: dllPath.Parent);

                process.AssertZeroExitCode();

                if (!File.Exists(tempFile) || new FileInfo(tempFile).Length == 0)
                {
                    throw new InvalidOperationException(
                        "OpenAPI generation produced no output. " +
                        "Ensure AddSwaggerGen() is configured in Program.cs.");
                }

                // Pretty-print if requested
                if (prettyPrint)
                {
                    Log.Debug("Pretty-printing JSON...");
                    var json = JToken.Parse(File.ReadAllText(tempFile));
                    File.WriteAllText(outputPath, json.ToString(Formatting.Indented));
                }
                else
                {
                    File.Move(tempFile, outputPath, overwrite: true);
                    tempFile = null; // Don't delete in finally
                }

                var fileSize = new FileInfo(outputPath).Length / 1024.0;
                Log.Information("OpenAPI documentation generated: {File} ({Size:N1} KB)", outputPath.Name, fileSize);
            }
            finally
            {
                // Clean up temp file if it still exists
                if (tempFile != null && File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
        finally
        {
            // Restore environment variables
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", previousAspNetEnv);
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", previousDotNetEnv);
        }
    }
}
