namespace Consilient.ProviderAssignments.Contracts.Import;

/// <summary>
/// Configuration settings for the provider assignment import process.
/// Bind to the "Import" section in appsettings.json.
/// </summary>
public class ProviderAssignmentsImportOptions
{
    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public const string SectionName = "Import";

    /// <summary>
    /// Number of rows to process per batch during import operations.
    /// Default is 1000.
    /// </summary>
    public int BatchSize { get; init; } = 1000;
}
