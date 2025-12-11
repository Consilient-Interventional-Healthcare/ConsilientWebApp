using Consilient.Infrastructure.ExcelImporter.Core;
using Consilient.Infrastructure.ExcelImporter.Domain;

namespace Consilient.Infrastructure.ExcelImporter.Factories
{
    public interface IImporterFactory
    {
        IExcelImporter<DoctorAssignment> Create(string connectionString, int facilityId, DateOnly serviceDate);
        IExcelImporter<DoctorAssignment> CreateWithSink(int facilityId, DateOnly serviceDate, IDataSink sink);
    }
}
