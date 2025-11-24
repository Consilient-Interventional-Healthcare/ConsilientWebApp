using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using static Consilient.Infrastructure.ExcelImporter.AssignmentImporter;

namespace Consilient.Infrastructure.ExcelImporter
{
    public class AssignmentImporter(ExcelImporterConfiguration configuration, ILogger<ExcelImporter> logger) : ExcelImporterBase<AssignmentData>(configuration, logger)
    {
        public class AssignmentData
        {
            public string Name { get; set; } = string.Empty;

            public string Location { get; set; } = string.Empty;

            public string HospitalNumber { get; set; } = string.Empty;

            public DateTime Admit { get; set; }

            public int? LOS { get; set; }

            public string? PsychEval { get; set; }

            public string? AttendingMD { get; set; }

            public string? MedicallyCleared { get; set; }

            public string? NursePractitioner { get; set; }

            public string? Insurance { get; set; }
        }

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

        private bool IsHeaderRow(IXLRow row, out List<string> foundHeaders)
        {
            var headers = row.CellsUsed().Select(c => c.GetValue<string>().Trim()).ToList();
            var hasRequiredHeaders = GetHeaders().All(header => headers.Contains(header, StringComparer.OrdinalIgnoreCase));
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


        protected override AssignmentData ExtractEntity(IXLRow row, IDictionary<string, int> columnMap)
        {
            var patient = new AssignmentData
            {
                Admit = row.Cell(columnMap[nameof(AssignmentData.Admit)]).GetValue<DateTime>(),
                AttendingMD = row.Cell(columnMap[nameof(AssignmentData.AttendingMD)]).GetValue<string?>(),
                HospitalNumber = row.Cell(columnMap[nameof(AssignmentData.HospitalNumber)]).GetValue<string>(),
                Insurance = row.Cell(columnMap[nameof(AssignmentData.Insurance)]).GetValue<string?>(),
                LOS = row.Cell(columnMap[nameof(AssignmentData.LOS)]).GetValue<int?>(),
                Location = row.Cell(columnMap[nameof(AssignmentData.Location)]).GetValue<string>(),
                MedicallyCleared = row.Cell(columnMap[nameof(AssignmentData.MedicallyCleared)]).GetValue<string?>(),
                Name = row.Cell(columnMap[nameof(AssignmentData.Name)]).GetValue<string>(),
                NursePractitioner = row.Cell(columnMap[nameof(AssignmentData.NursePractitioner)]).GetValue<string?>(),
                PsychEval = row.Cell(columnMap[nameof(AssignmentData.PsychEval)]).GetValue<string?>()
            };
            return patient;
        }
    }
}
