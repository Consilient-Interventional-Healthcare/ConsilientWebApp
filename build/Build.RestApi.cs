using Nuke.Common;
using Serilog;

/// <summary>
/// REST API code generation targets.
/// Handles: OpenAPI document generation and TypeScript client generation.
/// </summary>
partial class Build
{
    [Parameter("Skip namespace organization step")]
    readonly bool SkipOrganize;

    [Parameter("Database context (ConsilientDbContext, UsersDbContext, Both)")]
    readonly string DbContext = "Both";

    [Parameter("Skip database operations")]
    readonly bool SkipDatabase;

    Target GenerateOpenApiDoc => _ => _
        .DependsOn(Compile)
        .OnlyWhenDynamic(() => Force || NeedsRegeneration(OpenApiInputs, OpenApiFile))
        .Executes(() =>
        {
            var apiProject = Solution.GetProject("Consilient.Api");
            var dllPath = apiProject!.Directory / "bin" / Configuration / "net9.0" / "Consilient.Api.dll";

            SwaggerCliTool.GenerateDocument(dllPath, OpenApiFile);
        });

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

    Target GenerateApi => _ => _
        .DependsOn(GenerateApiTypes)
        .Description("Generate all REST API types (OpenAPI + TypeScript)");
}
