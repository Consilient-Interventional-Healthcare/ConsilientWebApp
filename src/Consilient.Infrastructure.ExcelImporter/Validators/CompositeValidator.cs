using Consilient.Infrastructure.ExcelImporter.Contracts;

namespace Consilient.Infrastructure.ExcelImporter.Validators
{

    public class CompositeValidator<TRow> : IRowValidator<TRow> where TRow : class
    {
        private readonly IEnumerable<IRowValidator<TRow>> _validators;

        public CompositeValidator(IEnumerable<IRowValidator<TRow>> validators)
        {
            _validators = validators ?? throw new ArgumentNullException(nameof(validators));
        }

        public ValidationResult Validate(TRow row, int rowNumber)
        {
            var allErrors = new List<ValidationError>();

            foreach (var validator in _validators)
            {
                var result = validator.Validate(row, rowNumber);
                allErrors.AddRange(result.Errors);
            }

            return allErrors.Count > 0
                ? ValidationResult.Failed(allErrors.ToArray())
                : ValidationResult.Success();
        }
    }

}