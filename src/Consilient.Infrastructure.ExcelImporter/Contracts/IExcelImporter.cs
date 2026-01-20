namespace Consilient.Infrastructure.ExcelImporter.Contracts
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
