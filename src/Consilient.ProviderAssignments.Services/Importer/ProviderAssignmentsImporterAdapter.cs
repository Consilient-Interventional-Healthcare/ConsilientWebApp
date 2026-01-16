using Consilient.ProviderAssignments.Contracts;
using Consilient.Infrastructure.ExcelImporter.Core;
using Consilient.Infrastructure.ExcelImporter.Models;

namespace Consilient.ProviderAssignments.Services.Importer
{
    internal class ProviderAssignmentsImporterAdapter(IExcelImporter<ExternalProviderAssignment> innerImporter) : IProviderAssignmentsImporter
    {
        public event EventHandler<ImportProgressEventArgs>? ProgressChanged
        {
            add => innerImporter.ProgressChanged += value;
            remove => innerImporter.ProgressChanged -= value;
        }

        public async Task<ImportResult> ImportAsync(Guid batchId, Stream stream, CancellationToken cancellationToken = default)
        {
            return await innerImporter.ImportAsync(batchId, stream, cancellationToken);
        }
    }
}
