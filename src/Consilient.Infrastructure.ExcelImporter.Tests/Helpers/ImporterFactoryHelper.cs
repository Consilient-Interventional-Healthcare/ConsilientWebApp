using Consilient.ProviderAssignments.Contracts;
using Consilient.ProviderAssignments.Services.Importer;
using Consilient.Infrastructure.ExcelImporter.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace Consilient.Infrastructure.ExcelImporter.Tests.Helpers
{
    internal static class ImporterFactoryHelper
    {

        public static IExcelImporter<ExternalProviderAssignment> CreateImporter(ISinkProvider sinkProvider, int facilityId, DateOnly serviceDate)
        {
            var importerFactory = new ImporterFactory(NullLoggerFactory.Instance, sinkProvider);
            var importer = importerFactory.Create(facilityId, serviceDate);
            return importer;
        }
    }
}
