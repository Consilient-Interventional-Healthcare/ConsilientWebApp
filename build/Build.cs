using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Information)
            .CreateLogger();
    }

    public static int Main() => Execute<Build>(x => x.Compile);

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
    // GRAPHQL PIPELINE TARGETS
    // ============================================

    Target GenerateGraphQLSchema => _ => _
        .DependsOn(Compile)
        .OnlyWhenDynamic(() => Force || NeedsRegeneration(GraphQLSchemaInputs, GraphQLSchemaFile))
        .Executes(() =>
        {
            var schemaExportProject = SourceDirectory / "Consilient.SchemaExport" / "Consilient.SchemaExport.csproj";

            GraphQLSchemaExporter.ExportSchema(schemaExportProject, GraphQLSchemaFile, Configuration);
        });

    Target GenerateGraphQLTypes => _ => _
        .DependsOn(GenerateGraphQLSchema)
        .OnlyWhenDynamic(() => Force || NeedsRegeneration([GraphQLSchemaFile], GraphQLTypesTempFile))
        .Executes(() =>
        {
            var nodeModules = GraphQLCodegenDir / "node_modules";

            // Install dependencies if needed
            if (!Directory.Exists(nodeModules))
            {
                Log.Information("Installing graphql-codegen dependencies...");
                var npmInstall = ProcessTasks.StartProcess(
                    "npm",
                    "install",
                    workingDirectory: GraphQLCodegenDir);
                npmInstall.AssertZeroExitCode();
            }

            // Run graphql-codegen (outputs to temp file)
            Log.Information("Generating GraphQL TypeScript types...");
            var npmGenerate = ProcessTasks.StartProcess(
                "npm",
                "run generate",
                workingDirectory: GraphQLCodegenDir);
            npmGenerate.AssertZeroExitCode();

            Log.Information("GraphQL TypeScript types generated: {File}", GraphQLTypesTempFile.Name);
        });

    // ============================================
    // REST API PIPELINE TARGETS
    // ============================================

    Target GenerateOpenApiDoc => _ => _
        .DependsOn(Compile)
        .OnlyWhenDynamic(() => Force || NeedsRegeneration(OpenApiInputs, OpenApiFile))
        .Executes(() =>
        {
            var apiProject = Solution.GetProject("Consilient.Api");
            var dllPath = apiProject!.Directory / "bin" / Configuration / "net9.0" / "Consilient.Api.dll";

            SwaggerCliTool.GenerateDocument(dllPath, OpenApiFile);
        });

    [Parameter("Skip namespace organization step")]
    readonly bool SkipOrganize;

    [Parameter("Database context (ConsilientDbContext, UsersDbContext, Both)")]
    readonly string DbContext = "Both";

    [Parameter("Skip database operations")]
    readonly bool SkipDatabase;

    Target GenerateApiTypes => _ => _
        .DependsOn(GenerateOpenApiDoc)
        .OnlyWhenDynamic(() => Force || NeedsRegeneration([OpenApiFile], RestTypesTempFile))
        .Executes(() =>
        {
            var apiProject = Solution.GetProject("Consilient.Api");
            var nswagConfig = apiProject!.Directory / "nswag.json";

            // Generate TypeScript from OpenAPI spec
            NSwagTool.GenerateTypeScript(
                OpenApiFile,
                nswagConfig,
                apiProject.Directory);

            // Organize into namespaces (output to temp file)
            if (!SkipOrganize)
            {
                TypeScriptNamespaceOrganizer.Organize(RestTypesTempFile, OpenApiFile);
            }
        });

    Target MergeTypes => _ => _
        .DependsOn(GenerateApiTypes, GenerateGraphQLTypes)
        .OnlyWhenDynamic(() => Force || NeedsRegeneration([RestTypesTempFile, GraphQLTypesTempFile], ApiTypesFile))
        .Executes(() =>
        {
            TypeScriptMerger.Merge(RestTypesTempFile, GraphQLTypesTempFile, ApiTypesFile);

            // Clean up temp files
            if (File.Exists(RestTypesTempFile))
            {
                File.Delete(RestTypesTempFile);
                Log.Debug("Deleted temp file: {File}", RestTypesTempFile.Name);
            }
            if (File.Exists(GraphQLTypesTempFile))
            {
                File.Delete(GraphQLTypesTempFile);
                Log.Debug("Deleted temp file: {File}", GraphQLTypesTempFile.Name);
            }
        });

    // ============================================
    // AGGREGATE TARGETS
    // ============================================

    Target GenerateGraphQL => _ => _
        .DependsOn(GenerateGraphQLTypes)
        .Description("Generate all GraphQL types (schema + TypeScript)");

    Target GenerateApi => _ => _
        .DependsOn(GenerateApiTypes)
        .Description("Generate all REST API types (OpenAPI + TypeScript)");

    Target GenerateAllTypes => _ => _
        .DependsOn(MergeTypes)
        .Description("Generate all types (GraphQL + REST API merged into single file)");

    // ============================================
    // TESTING TARGETS
    // ============================================

    Target Test => _ => _
        .DependsOn(Compile)
        .Description("Run all backend tests")
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .SetResultsDirectory(TestResultsDirectory)
                .SetLoggers("trx;LogFileName=test-results.trx"));
        });

    // ============================================
    // FRONTEND TARGETS
    // ============================================

    Target RestoreFrontend => _ => _
        .Description("Install frontend npm dependencies")
        .Executes(() =>
        {
            RunNpm("install", WebApp2Directory);
        });

    Target LintFrontend => _ => _
        .DependsOn(RestoreFrontend)
        .Description("Run ESLint on frontend code")
        .Executes(() =>
        {
            RunNpm("run lint", WebApp2Directory);
        });

    Target TestFrontend => _ => _
        .DependsOn(RestoreFrontend)
        .Description("Run frontend tests with Vitest")
        .Executes(() =>
        {
            RunNpm("run test", WebApp2Directory);
        });

    Target BuildFrontend => _ => _
        .DependsOn(RestoreFrontend, GenerateAllTypes)
        .Description("Build frontend for production (requires generated types)")
        .Executes(() =>
        {
            RunNpm("run build", WebApp2Directory);
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
