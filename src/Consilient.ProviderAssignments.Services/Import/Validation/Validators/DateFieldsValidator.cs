using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.Infrastructure.ExcelImporter.Validators;
using Consilient.ProviderAssignments.Contracts;
using Consilient.ProviderAssignments.Contracts.Import;

namespace Consilient.ProviderAssignments.Services.Import.Validation.Validators
{
    public class DateFieldsValidator(Func<DateTime>? utcNow = null) : RowValidator<ExcelProviderAssignmentRow>, IExcelRowValidator
    {
        private readonly Func<DateTime> _utcNow = utcNow ?? (() => DateTime.UtcNow);

        public override ValidationResult Validate(ExcelProviderAssignmentRow row, int rowNumber)
        {
            var errors = new List<ValidationError>();
            var now = _utcNow();

            if (row.Admit > now)
                errors.Add(Error(rowNumber, nameof(row.Admit), "Admit date cannot be in the future"));

            if (row.Dob.HasValue && row.Dob.Value > DateOnly.FromDateTime(now))
                errors.Add(Error(rowNumber, nameof(row.Dob), "Date of birth cannot be in the future"));

            return errors.Count > 0
                ? ValidationResult.Failed([.. errors])
                : ValidationResult.Success();
        }
    }
}
