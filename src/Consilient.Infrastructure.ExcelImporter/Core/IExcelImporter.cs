using Consilient.Infrastructure.ExcelImporter.Models;

namespace Consilient.Infrastructure.ExcelImporter.Core
{
    public interface IExcelImporter<TRow> where TRow : class
    {
        event EventHandler<ImportProgressEventArgs>? ProgressChanged;

        /// <summary>
        /// Imports data from a stream.
        /// </summary>
        Task<ImportResult> ImportAsync(Guid batchId, Stream stream, CancellationToken cancellationToken = default);
    }
}
