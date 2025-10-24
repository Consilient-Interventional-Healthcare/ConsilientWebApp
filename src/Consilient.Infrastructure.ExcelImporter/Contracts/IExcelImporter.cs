using Consilient.Infrastructure.ExcelImporter.Models;

namespace Consilient.Infrastructure.ExcelImporter.Contracts
{
    public interface IExcelImporter
    {
        public IEnumerable<PatientData> Import(string filename);
    }
}
