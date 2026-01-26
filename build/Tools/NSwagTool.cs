using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Serilog;

/// <summary>
/// Wrapper for NSwag.MSBuild to generate TypeScript from OpenAPI specifications.
/// </summary>
public static class NSwagTool
{
    private const string DefaultNSwagVersion = "14.6.3";

    /// <summary>
    /// Generates TypeScript types from an OpenAPI specification using NSwag.
    /// </summary>
    /// <param name="openApiFile">Path to the OpenAPI JSON file</param>
    /// <param name="nswagConfigFile">Path to the nswag.json configuration file</param>
    /// <param name="workingDirectory">Working directory for NSwag execution (usually API project directory)</param>
    /// <param name="targetFramework">Target framework for NSwag runtime (default: Net90)</param>
    /// <param name="nswagVersion">NSwag.MSBuild package version (default: 14.6.3)</param>
    public static void GenerateTypeScript(
        AbsolutePath openApiFile,
        AbsolutePath nswagConfigFile,
        AbsolutePath workingDirectory,
        string targetFramework = "Net90",
        string nswagVersion = DefaultNSwagVersion)
    {
        if (!File.Exists(openApiFile))
        {
            throw new FileNotFoundException($"OpenAPI file not found: {openApiFile}");
        }

        if (!File.Exists(nswagConfigFile))
        {
            throw new FileNotFoundException($"NSwag config not found: {nswagConfigFile}");
        }

        // Find NSwag DLL in NuGet cache
        var nswagDll = FindNSwagDll(targetFramework, nswagVersion);

        // NSwag expects swagger.json in the working directory (per nswag.json config)
        var swaggerJson = workingDirectory / "swagger.json";
        var createdSwaggerJson = false;

        try
        {
            // Copy OpenAPI file to swagger.json in working directory
            if (!File.Exists(swaggerJson) || !AbsolutePath.Create(swaggerJson).Equals(openApiFile))
            {
                Log.Debug("Copying OpenAPI spec to {SwaggerJson}", swaggerJson);
                File.Copy(openApiFile, swaggerJson, overwrite: true);
                createdSwaggerJson = true;
            }

            Log.Information("Generating TypeScript from OpenAPI spec...");

            var process = ProcessTasks.StartProcess(
                "dotnet",
                $"\"{nswagDll}\" run \"{nswagConfigFile}\"",
                workingDirectory: workingDirectory);

            process.AssertZeroExitCode();

            Log.Information("TypeScript types generated successfully");
        }
        finally
        {
            // Clean up swagger.json if we created it
            if (createdSwaggerJson && File.Exists(swaggerJson))
            {
                Log.Debug("Cleaning up {SwaggerJson}", swaggerJson);
                File.Delete(swaggerJson);
            }
        }
    }

    /// <summary>
    /// Finds the NSwag.MSBuild DLL in the NuGet package cache.
    /// </summary>
    private static string FindNSwagDll(string targetFramework, string version)
    {
        // Get NuGet packages folder
        var nugetPackages = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
        if (string.IsNullOrEmpty(nugetPackages))
        {
            nugetPackages = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget", "packages");
        }

        var nswagDll = Path.Combine(
            nugetPackages,
            "nswag.msbuild",
            version,
            "tools",
            targetFramework,
            "dotnet-nswag.dll");

        if (!File.Exists(nswagDll))
        {
            throw new FileNotFoundException(
                $"NSwag.MSBuild v{version} not found at: {nswagDll}\n" +
                $"Ensure the package is installed: <PackageReference Include=\"NSwag.MSBuild\" Version=\"{version}\" />\n" +
                "Then run: dotnet restore");
        }

        Log.Debug("Found NSwag at: {NSwagPath}", nswagDll);
        return nswagDll;
    }
}
