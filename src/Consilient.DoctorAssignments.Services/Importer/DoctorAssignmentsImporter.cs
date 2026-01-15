using Consilient.DoctorAssignments.Contracts;
using Consilient.Infrastructure.ExcelImporter.Core;
using Consilient.Infrastructure.ExcelImporter.Models;

namespace Consilient.DoctorAssignments.Services.Importer
{
    internal class DoctorAssignmentsImporter(IExcelImporter<ExternalDoctorAssignment> innerImporter) : IDoctorAssignmentsImporter
    {
        private readonly IExcelImporter<ExternalDoctorAssignment> _innerImporter = innerImporter;

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
