using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Serilog;

/// <summary>
/// Exports GraphQL schema by running the Consilient.SchemaExport console app.
/// Uses dotnet run to properly handle all .NET dependency resolution.
/// </summary>
public static class GraphQLSchemaExporter
{
    /// <summary>
    /// Exports the GraphQL schema to SDL format by running the SchemaExport project.
    /// </summary>
    /// <param name="schemaExportProject">Path to Consilient.SchemaExport.csproj</param>
    /// <param name="outputPath">Path where schema.graphql will be written</param>
    /// <param name="configuration">Build configuration (Debug/Release)</param>
    public static void ExportSchema(AbsolutePath schemaExportProject, AbsolutePath outputPath, string configuration)
    {
        if (!File.Exists(schemaExportProject))
        {
            throw new FileNotFoundException($"Schema export project not found: {schemaExportProject}");
        }

        // Ensure output directory exists
        outputPath.Parent.CreateDirectory();

        Log.Information("Generating GraphQL schema...");
        Log.Debug("Project: {Project}", schemaExportProject);
        Log.Debug("Output: {Output}", outputPath);

        // Run the schema export console app
        // The project takes the output path as an argument
        var process = ProcessTasks.StartProcess(
            "dotnet",
            $"run --project \"{schemaExportProject}\" --configuration {configuration} --no-build -- \"{outputPath}\"",
            workingDirectory: schemaExportProject.Parent);

        process.AssertZeroExitCode();

        if (!File.Exists(outputPath))
        {
            throw new FileNotFoundException(
                "GraphQL schema generation completed but output file not created. " +
                "Check SchemaExport console output for errors.");
        }

        var fileSize = new FileInfo(outputPath).Length / 1024.0;
        Log.Information("GraphQL schema exported: {File} ({Size:N1} KB)", outputPath.Name, fileSize);
    }
}
