using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

/// <summary>
/// Testing targets.
/// Handles: Backend and frontend test execution.
/// </summary>
partial class Build
{
    Target Test => _ => _
        .DependsOn(Compile)
        .Description("Run all backend tests")
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .SetResultsDirectory(TestResultsDirectory)
                .SetLoggers("trx;LogFileName=test-results.trx"));
        });

    Target TestFrontend => _ => _
        .DependsOn(RestoreFrontend)
        .Description("Run frontend tests with Vitest")
        .Executes(() =>
        {
            RunNpm("run test", WebApp2Directory);
        });
}
