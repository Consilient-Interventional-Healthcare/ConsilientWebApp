using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.ProviderAssignments.Contracts;

namespace Consilient.ProviderAssignments.Contracts.Import
{
    /// <summary>
    /// Validates raw Excel row data during import.
    /// Extends IRowValidator to be directly usable by the import pipeline.
    /// </summary>
    public interface IExcelRowValidator : IRowValidator<ExcelProviderAssignmentRow>
    {
        // Inherits: ValidationResult Validate(ExcelProviderAssignmentRow row, int rowNumber);
    }
}
