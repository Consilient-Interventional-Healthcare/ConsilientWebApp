using Consilient.Infrastructure.ExcelImporter.Models;

namespace Consilient.Infrastructure.ExcelImporter.Core
{
    public interface IExcelImporter<TRow> where TRow : class
    {
        event EventHandler<ImportProgressEventArgs>? ProgressChanged;
        Task<ImportResult> ImportAsync(string sourceFile, CancellationToken cancellationToken = default);
    }
}
