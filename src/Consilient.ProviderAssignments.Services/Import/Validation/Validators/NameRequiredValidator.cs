using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.Infrastructure.ExcelImporter.Validators;
using Consilient.ProviderAssignments.Contracts;

namespace Consilient.ProviderAssignments.Services.Import.Validation.Validators
{
    public class NameRequiredValidator : RowValidator<ExcelProviderAssignmentRow>, IExcelRowValidator
    {
        public override ValidationResult Validate(ExcelProviderAssignmentRow row, int rowNumber)
        {
            if (string.IsNullOrWhiteSpace(row.Name))
                return ValidationResult.Failed(Error(rowNumber, nameof(row.Name), "Name is required"));

            return ValidationResult.Success();
        }
    }
}
