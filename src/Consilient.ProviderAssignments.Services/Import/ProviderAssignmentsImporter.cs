using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.ProviderAssignments.Contracts;
using Consilient.ProviderAssignments.Contracts.Import;

namespace Consilient.ProviderAssignments.Services.Import
{
    internal class ProviderAssignmentsImporter(IExcelImporter<ProcessedProviderAssignment> innerImporter) : IProviderAssignmentsImporter
    {
        private readonly IExcelImporter<ProcessedProviderAssignment> _innerImporter = innerImporter;

        public event EventHandler<ImportProgressEventArgs>? ProgressChanged
        {
            add => _innerImporter.ProgressChanged += value;
            remove => _innerImporter.ProgressChanged -= value;
        }

        public async Task<ImportResult> ImportAsync(Guid batchId, Stream stream, CancellationToken cancellationToken = default)
        {
            var result = await _innerImporter.ImportAsync(batchId, stream, cancellationToken);
            return result;
        }
    }
}
