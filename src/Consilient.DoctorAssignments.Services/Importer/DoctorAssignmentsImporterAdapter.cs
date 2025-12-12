using Consilient.DoctorAssignments.Contracts;
using Consilient.Infrastructure.ExcelImporter.Core;
using Consilient.Infrastructure.ExcelImporter.Models;

namespace Consilient.DoctorAssignments.Services.Importer
{
    internal class DoctorAssignmentsImporterAdapter(IExcelImporter<ExternalDoctorAssignment> innerImporter) : IDoctorAssignmentsImporter
    {
        public event EventHandler<ImportProgressEventArgs>? ProgressChanged
        {
            add => innerImporter.ProgressChanged += value;
            remove => innerImporter.ProgressChanged -= value;
        }

        public async Task<ImportResult> ImportAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return await innerImporter.ImportAsync(filePath, cancellationToken);
        }
    }
}
