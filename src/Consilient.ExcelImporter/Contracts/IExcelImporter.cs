using Consilient.ExcelImporter.Models;

namespace Consilient.ExcelImporter.Contracts
{
    public interface IExcelImporter
    {
        public IEnumerable<PatientData> Import(string filename);
    }
}
