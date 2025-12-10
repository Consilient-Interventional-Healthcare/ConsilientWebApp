namespace Consilient.Infrastructure.ExcelImporter.Contracts
{
    internal interface IExcelDataMapper<TData> where TData : class, new()
    {
        IEnumerable<TData> Map(SheetReadResult sheetReadResult);
    }
}