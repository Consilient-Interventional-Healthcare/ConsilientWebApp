using Consilient.Infrastructure.ExcelImporter.Models;

namespace Consilient.Infrastructure.ExcelImporter.Core;

public interface IRowValidator<TRow> where TRow : class
{
    ValidationResult Validate(TRow row, int rowNumber);
}
