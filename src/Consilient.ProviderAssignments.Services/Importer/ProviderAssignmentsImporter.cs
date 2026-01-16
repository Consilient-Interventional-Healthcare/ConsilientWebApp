using Consilient.ProviderAssignments.Contracts;
using Consilient.Infrastructure.ExcelImporter.Core;
using Consilient.Infrastructure.ExcelImporter.Models;

namespace Consilient.ProviderAssignments.Services.Importer
{
    internal class ProviderAssignmentsImporter(IExcelImporter<ExternalProviderAssignment> innerImporter) : IProviderAssignmentsImporter
    {
        private readonly IExcelImporter<ExternalProviderAssignment> _innerImporter = innerImporter;

        public event EventHandler<ImportProgressEventArgs>? ProgressChanged
        {
            add => _innerImporter.ProgressChanged += value;
            remove => _innerImporter.ProgressChanged -= value;
        }

        public async Task<ImportResult> ImportAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var result = await _innerImporter.ImportAsync(stream, cancellationToken);
            return result;
        }
    }
}
