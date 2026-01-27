namespace Consilient.Infrastructure.ExcelImporter.Contracts;

public interface IRowMapper<TRow> where TRow : class
{
    Result<TRow> Map(ExcelRow excelRow, ColumnMapping mapping);
}
