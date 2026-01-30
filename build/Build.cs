using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using Serilog.Events;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build : NukeBuild
{
    static Build()
    {
        // Enable auto-flush on Console.Out for real-time subprocess output
        Console.Out.Flush();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Information,
                standardErrorFromLevel: null)  // Keep all output on stdout
            .CreateLogger();
    }

    public static int Main()
    {
        try
        {
            return Execute<Build>(x => x.Compile);
        }
        finally
        {
            Log.CloseAndFlush();
            Console.Out.Flush();
        }
    }

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly string Configuration = IsLocalBuild ? "Debug" : "Release";

    [Parameter("Force regeneration even if outputs are up-to-date")]
    readonly bool Force;

    [Solution]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    readonly Solution Solution;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    // Paths
    static AbsolutePath SourceDirectory => RootDirectory / "src";
    static AbsolutePath DocsDirectory => RootDirectory / "docs";
    static AbsolutePath WebApp2Directory => SourceDirectory / "Consilient.WebApp2";
    static AbsolutePath WebApp2TypesDir => WebApp2Directory / "src" / "types";
    static AbsolutePath GraphQLCodegenDir => RootDirectory / "build" / "graphql-codegen";
    static AbsolutePath TestResultsDirectory => RootDirectory / "test-results";

    // Docker/Database paths
    static AbsolutePath DockerDirectory => SourceDirectory / ".docker";
    static AbsolutePath DockerComposeFile => DockerDirectory / "docker-compose.yml";
    static AbsolutePath MigrationsProject => SourceDirectory / "Consilient.Data.Migrations";

    // Output files
    static AbsolutePath GraphQLSchemaFile => DocsDirectory / "schema.graphql";
    static AbsolutePath OpenApiFile => DocsDirectory / "openapi.json";

    // Intermediate files (for merging)
    static AbsolutePath RestTypesTempFile => WebApp2TypesDir / ".rest.temp.ts";
    static AbsolutePath GraphQLTypesTempFile => WebApp2TypesDir / ".graphql.temp.ts";

    // Final merged output
    static AbsolutePath ApiTypesFile => WebApp2TypesDir / "api.generated.ts";

    // Input patterns for incremental checks
    static IEnumerable<AbsolutePath> GraphQLSchemaInputs =>
        (SourceDirectory / "Consilient.Data.GraphQL").GlobFiles("**/*.cs")
        .Concat((SourceDirectory / "Consilient.Data").GlobFiles("**/*.cs"))
        .Concat((SourceDirectory / "Consilient.SchemaExport").GlobFiles("**/*.cs"));

    static IEnumerable<AbsolutePath> OpenApiInputs =>
        (SourceDirectory / "Consilient.Api").GlobFiles("**/*.cs")
        .Concat(SourceDirectory.GlobFiles("Consilient.*.Contracts/**/*.cs"));

    // ============================================
    // CORE BUILD TARGETS
    // ============================================

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            DotNetClean(s => s
                .SetProject(Solution)
                .SetConfiguration(Configuration));
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });


    // ============================================
    // HELPER METHODS
    // ============================================


    static void RunNpm(string arguments, AbsolutePath workingDirectory)
    {
        Log.Information("Running npm {Arguments} in {Directory}...", arguments, workingDirectory.Name);
        var process = ProcessTasks.StartProcess(
            "npm",
            arguments,
            workingDirectory: workingDirectory);
        process.AssertZeroExitCode();
    }

    static bool NeedsRegeneration(IEnumerable<AbsolutePath> inputs, AbsolutePath output)
    {
        if (!output.FileExists())
        {
            Log.Information("Output {Output} does not exist, regeneration needed", output.Name);
            return true;
        }

        var outputTime = File.GetLastWriteTimeUtc(output);
        var inputList = inputs.ToList();
        var newerInputs = inputList
            .Where(i => i.FileExists() && File.GetLastWriteTimeUtc(i) > outputTime)
            .ToList();

        if (newerInputs.Count > 0)
        {
            Log.Information("Found {Count} newer input files, regeneration needed", newerInputs.Count);
            newerInputs.Take(5).ForEach(f => Log.Debug("  Newer: {File}", f.Name));
            return true;
        }

        Log.Information("Output {Output} is up-to-date ({Count} inputs checked)", output.Name, inputList.Count);
        return false;
    }
}
