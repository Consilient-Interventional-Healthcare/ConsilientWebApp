using Consilient.Infrastructure.ExcelImporter.Core;
using Consilient.Infrastructure.ExcelImporter.Domain;
using Consilient.Infrastructure.ExcelImporter.Factories;
using Consilient.Infrastructure.ExcelImporter.Sinks;
using Microsoft.Extensions.Logging.Abstractions;

namespace Consilient.Infrastructure.ExcelImporter.Tests.Helpers
{
    internal static class ImporterFactoryHelper
    {
        public static IExcelImporter<DoctorAssignment> CreateImporterWithInMemorySink(int facilityId, DateOnly serviceDate)
        {
            return CreateImporterWithInMemorySink(facilityId, serviceDate, new InMemorySink<DoctorAssignment>());
        }

        public static IExcelImporter<DoctorAssignment> CreateImporterWithInMemorySink(int facilityId, DateOnly serviceDate, IDataSink dataSink)
        {
            var importerFactory = new ImporterFactory(NullLoggerFactory.Instance);
            var importer = importerFactory.CreateWithSink(facilityId, serviceDate, dataSink);
            return importer;
        }
    }
}
