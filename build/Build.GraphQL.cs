using Nuke.Common;
using Nuke.Common.Tooling;
using Serilog;

/// <summary>
/// GraphQL code generation targets.
/// Handles: GraphQL schema export and TypeScript type generation.
/// </summary>
partial class Build
{
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

    Target GenerateGraphQL => _ => _
        .DependsOn(GenerateGraphQLTypes)
        .Description("Generate all GraphQL types (schema + TypeScript)");
}
