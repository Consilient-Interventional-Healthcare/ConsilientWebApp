namespace Consilient.Infrastructure.ExcelImporter.Contracts;


public interface IRowValidator<TRow> where TRow : class
{
    ValidationResult Validate(TRow row, int rowNumber);
}
