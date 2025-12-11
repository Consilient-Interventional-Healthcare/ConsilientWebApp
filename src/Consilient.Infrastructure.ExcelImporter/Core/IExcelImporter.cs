using Consilient.Infrastructure.ExcelImporter.Models;

namespace Consilient.Infrastructure.ExcelImporter.Core
{
    public interface IExcelImporter<TRow> where TRow : class
    {
        Task<ImportResult> ImportAsync(
            string sourceFile,
            IDataSink destination,
            ImportOptions options,
            IProgress<ImportProgress>? progress = null,
            CancellationToken cancellationToken = default);
    }
}
