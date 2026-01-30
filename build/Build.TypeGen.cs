using Nuke.Common;
using Serilog;

/// <summary>
/// Type generation orchestration.
/// Handles: Merging GraphQL and REST API TypeScript types into a single output file.
/// </summary>
partial class Build
{
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

    Target GenerateAllTypes => _ => _
        .DependsOn(MergeTypes)
        .Description("Generate all types (GraphQL + REST API merged into single file)");
}
