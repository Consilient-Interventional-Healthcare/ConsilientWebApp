using Nuke.Common;
using Nuke.Common.IO;

/// <summary>
/// OpenAPI/REST code generation targets.
/// Handles: OpenAPI document generation and TypeScript type generation.
/// </summary>
partial class Build
{
    // OpenAPI targets will be moved here from Build.cs in later steps.
    //
    // Targets to migrate:
    // - GenerateOpenApiDoc: Export OpenAPI JSON from compiled API assembly
    // - GenerateApiTypes: Generate TypeScript types from OpenAPI spec
    // - GenerateApi: Aggregate target for full OpenAPI pipeline
    // - GenerateAllTypes: Master aggregate for all code generation
}
