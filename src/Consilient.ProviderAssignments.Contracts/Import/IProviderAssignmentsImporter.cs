using Consilient.Infrastructure.ExcelImporter.Contracts;

namespace Consilient.ProviderAssignments.Contracts.Import;

/// <summary>
/// Imports provider assignment data from Excel files into the staging table.
/// Extends the generic Excel importer with ProcessedProviderAssignment as the output type.
/// </summary>
public interface IProviderAssignmentsImporter : IExcelImporter<ProcessedProviderAssignment>
{
}
