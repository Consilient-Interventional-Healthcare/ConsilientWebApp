using Consilient.Infrastructure.ExcelImporter.Models;

namespace Consilient.Infrastructure.ExcelImporter.Core
{
    public interface IRowMapper<TRow> where TRow : class
    {
        Result<TRow> Map(ExcelRow excelRow, ColumnMapping mapping);
    }
}
