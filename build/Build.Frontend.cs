using Nuke.Common;

/// <summary>
/// Frontend build targets.
/// Handles: npm package management, linting, and production builds.
/// </summary>
partial class Build
{
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

    Target BuildFrontend => _ => _
        .DependsOn(RestoreFrontend, GenerateAllTypes)
        .Description("Build frontend for production (requires generated types)")
        .Executes(() =>
        {
            RunNpm("run build", WebApp2Directory);
        });
}
