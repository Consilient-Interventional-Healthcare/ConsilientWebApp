namespace Consilient.Infrastructure.ExcelImporter.Contracts
{
    public interface IExcelReader
    {
        IAsyncEnumerable<ExcelRow> ReadRowsAsync(
            Stream stream,
            SheetSelector sheet,
            CancellationToken cancellationToken = default);
    }
}
