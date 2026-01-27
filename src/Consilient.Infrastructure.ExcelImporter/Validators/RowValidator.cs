using Consilient.Infrastructure.ExcelImporter.Contracts;

namespace Consilient.Infrastructure.ExcelImporter.Validators;


public abstract class RowValidator<TRow> : IRowValidator<TRow> where TRow : class
{
    public abstract ValidationResult Validate(TRow row, int rowNumber);

    protected ValidationError Error(int rowNumber, string propertyName, string message) =>
        new(rowNumber, propertyName, message, ValidationSeverity.Error);

    protected ValidationError Warning(int rowNumber, string propertyName, string message) =>
        new(rowNumber, propertyName, message, ValidationSeverity.Warning);

    protected ValidationError Critical(int rowNumber, string propertyName, string message) =>
        new(rowNumber, propertyName, message, ValidationSeverity.Critical);
}