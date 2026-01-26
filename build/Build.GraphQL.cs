/// <summary>
/// GraphQL code generation targets.
/// Handles: GraphQL schema export and TypeScript type generation.
/// </summary>
partial class Build
{
    // GraphQL targets will be moved here from Build.cs in later steps.
    //
    // Targets to migrate:
    // - GenerateGraphQLSchema: Export GraphQL SDL from EntityGraphQL
    // - GenerateGraphQLTypes: Generate TypeScript types from schema
    // - GenerateGraphQL: Aggregate target for full GraphQL pipeline
}
