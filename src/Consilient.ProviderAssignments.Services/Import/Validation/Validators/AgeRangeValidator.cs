using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.Infrastructure.ExcelImporter.Validators;
using Consilient.ProviderAssignments.Contracts;

namespace Consilient.ProviderAssignments.Services.Import.Validation.Validators
{
    public class AgeRangeValidator : RowValidator<ExcelProviderAssignmentRow>, IExcelRowValidator
    {
        public override ValidationResult Validate(ExcelProviderAssignmentRow row, int rowNumber)
        {
            if (row.Age < 0 || row.Age > 150)
                return ValidationResult.Failed(Error(rowNumber, nameof(row.Age), "Age must be between 0 and 150"));

            return ValidationResult.Success();
        }
    }
}
