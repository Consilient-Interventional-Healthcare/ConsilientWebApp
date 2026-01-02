using Consilient.Infrastructure.ExcelImporter.Models;

namespace Consilient.Infrastructure.ExcelImporter.Core
{
    public interface IExcelReader
    {
        IAsyncEnumerable<ExcelRow> ReadRowsAsync(
            Stream stream,
            SheetSelector sheet,
            CancellationToken cancellationToken = default);
    }
}
