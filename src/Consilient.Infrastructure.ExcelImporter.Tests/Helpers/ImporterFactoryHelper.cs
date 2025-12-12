using Consilient.DoctorAssignments.Contracts;
using Consilient.DoctorAssignments.Services.Importer;
using Consilient.Infrastructure.ExcelImporter.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace Consilient.Infrastructure.ExcelImporter.Tests.Helpers
{
    internal static class ImporterFactoryHelper
    {

        public static IExcelImporter<ExternalDoctorAssignment> CreateImporter(ISinkProvider sinkProvider, int facilityId, DateOnly serviceDate)
        {
            var importerFactory = new ImporterFactory(NullLoggerFactory.Instance, sinkProvider);
            var importer = importerFactory.Create(facilityId, serviceDate);
            return importer;
        }
    }
}
