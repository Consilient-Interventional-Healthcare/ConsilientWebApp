using System.Text.RegularExpressions;

/// <summary>
/// Centralized configuration constants for the build system.
/// To port this build to another project, update the values in this file.
/// </summary>
partial class Build
{
    // ============================================
    // DATABASE CONFIGURATION
    // ============================================

    /// <summary>Main application database name.</summary>
    const string MainDatabaseName = "consilient_main";

    /// <summary>Users/identity database name.</summary>
    const string UsersDatabaseName = "consilient_users";

    /// <summary>Database schemas for documentation generation.</summary>
    static readonly string[] DatabaseSchemas = ["billing", "clinical", "compensation", "identity", "staging"];

    /// <summary>Maps DbContext names to their target database names.</summary>
    static readonly Dictionary<string, string> DbContextToDatabaseMap = new()
    {
        ["ConsilientDbContext"] = MainDatabaseName,
        ["UsersDbContext"] = UsersDatabaseName
    };

    // ============================================
    // DOCKER DEFAULTS
    // ============================================

    /// <summary>Default Docker container name for the database.</summary>
    const string DefaultDatabaseContainerName = "consilient.dbs.container";

    /// <summary>Default path to docker-compose file (relative to root).</summary>
    const string DefaultDbComposeFile = "src/.docker/docker-compose.yml";

    /// <summary>Default Docker service name for the database.</summary>
    const string DefaultDbServiceName = "db";

    /// <summary>Default database server port.</summary>
    const int DefaultDbPort = 1434;

    // ============================================
    // RETRY/TIMEOUT CONFIGURATION
    // ============================================

    /// <summary>Default health check interval in seconds.</summary>
    const int DefaultHealthCheckIntervalSeconds = 5;

    /// <summary>Default retry count for quick operations (60 seconds total).</summary>
    const int DefaultHealthCheckRetries = 12;

    /// <summary>Retry count for operations after container start (75 seconds total).</summary>
    const int ContainerStartRetries = 15;

    /// <summary>Retry count for operations after database rebuild (120 seconds total).</summary>
    const int DatabaseRebuildRetries = 24;

    // ============================================
    // TERRAFORM DEFAULTS
    // ============================================

    /// <summary>Default resource group for Terraform state storage.</summary>
    const string DefaultTerraformStateRg = "consilient-terraform";

    /// <summary>Default storage account for Terraform state.</summary>
    const string DefaultTerraformStateSa = "consilienttfstate";

    /// <summary>Default container name for Terraform state.</summary>
    const string DefaultTerraformStateContainer = "tfstate";

    /// <summary>Default Azure region.</summary>
    const string DefaultAzureRegion = "canadacentral";

    // ============================================
    // SQL TOOL PATHS (for Docker execution)
    // ============================================

    /// <summary>Path to sqlcmd inside the database container.</summary>
    const string SqlCmdPathInContainer = "/opt/mssql-tools18/bin/sqlcmd";

    /// <summary>Temp directory inside the container for SQL scripts.</summary>
    const string ContainerTempDir = "/tmp";

    // ============================================
    // HELPER METHODS
    // ============================================

    /// <summary>
    /// Extracts the short name from a DbContext name.
    /// E.g., "ConsilientDbContext" -> "Consilient"
    /// </summary>
    static string GetDbContextShortName(string contextName)
    {
        var match = Regex.Match(contextName, @"^(.*)DbContext$");
        return match.Success ? match.Groups[1].Value : contextName;
    }

    /// <summary>
    /// Gets the database name for a given DbContext.
    /// </summary>
    static string GetDatabaseForContext(string contextName) =>
        DbContextToDatabaseMap.TryGetValue(contextName, out var db) ? db : MainDatabaseName;
}
