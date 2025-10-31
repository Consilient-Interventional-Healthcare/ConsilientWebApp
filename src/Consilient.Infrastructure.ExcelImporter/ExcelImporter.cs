using ClosedXML.Excel;
using Consilient.Infrastructure.ExcelImporter.Constants;
using Consilient.Infrastructure.ExcelImporter.Models;
using Microsoft.Extensions.Logging;

namespace Consilient.Infrastructure.ExcelImporter
{
    public class ExcelImporter(ExcelImporterConfiguration configuration, ILogger<ExcelImporter> logger) : ExcelImporterBase<PatientData>(configuration, logger)
    {
        protected override (IXLRow? headerRow, Dictionary<string, int>? columnMap) FindHeader(IXLWorksheet worksheet)
        {
            foreach (var row in worksheet.RowsUsed())
            {
                if (!IsHeaderRow(row, out var foundHeaders))
                {
                    continue;
                }
                var columnMap = GetColumnMap(foundHeaders);
                Logger.LogDebug("Header row found on row {RowNumber} in worksheet '{WorksheetName}'.", row.RowNumber(), worksheet.Name);
                return (row, columnMap);
            }
            return (null, null);
        }

        private static bool IsHeaderRow(IXLRow row, out List<string> foundHeaders)
        {
            var headers = row.CellsUsed().Select(c => c.GetValue<string>().Trim()).ToList();
            var hasRequiredHeaders = ExcelHeader.ExpectedHeaders.All(header => headers.Contains(header, StringComparer.OrdinalIgnoreCase));
            foundHeaders = headers;
            return hasRequiredHeaders;
        }

        private static Dictionary<string, int> GetColumnMap(List<string> headers)
        {
            return headers
                .Select((header, index) => new { header, index = index + 1 }) // +1 for 1-based cell index
                .Where(x => !string.IsNullOrWhiteSpace(x.header))
                .ToDictionary(x => x.header, x => x.index, StringComparer.OrdinalIgnoreCase);
        }


        protected override PatientData ExtractEntity(IXLRow row, IDictionary<string, int> columnMap)
        {
            var patient = new PatientData
            {
                CaseId = row.Cell(columnMap[ExcelHeader.CaseId]).GetValue<string>(),
                Name = row.Cell(columnMap[ExcelHeader.Name]).GetValue<string>(),
                Mrn = row.Cell(columnMap[ExcelHeader.Mrn]).GetValue<string>(),
                Sex = row.Cell(columnMap[ExcelHeader.Sex]).GetValue<string>(),
                Age = row.Cell(columnMap[ExcelHeader.Age]).GetValue<int>(),
                Dob = row.Cell(columnMap[ExcelHeader.Dob]).GetValue<DateTime?>(),
                Room = row.Cell(columnMap[ExcelHeader.Room]).GetValue<string>(),
                Bed = row.Cell(columnMap[ExcelHeader.Bed]).GetValue<string>(),
                Doa = row.Cell(columnMap[ExcelHeader.Doa]).GetValue<DateTime>(),
                Los = row.Cell(columnMap[ExcelHeader.Los]).GetValue<int>(),
                AttendingPhysician = row.Cell(columnMap[ExcelHeader.AttendingPhysician]).GetValue<string>(),
                PrimaryInsurance = row.Cell(columnMap[ExcelHeader.PrimaryInsurance]).GetValue<string>(),
                AdmDx = row.Cell(columnMap[ExcelHeader.AdmDx]).GetValue<string>()
            };
            return patient;
        }
    }
}