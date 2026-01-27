using Consilient.Infrastructure.ExcelImporter.Contracts;

namespace Consilient.ProviderAssignments.Contracts.Import;

/// <summary>
/// Provides data sinks for writing imported provider assignment records.
/// </summary>
public interface ISinkProvider
{
    /// <summary>
    /// Gets the configured data sink for writing imported records.
    /// </summary>
    /// <returns>A data sink implementation (e.g., EF Core staging table sink).</returns>
    IDataSink GetSink();
}