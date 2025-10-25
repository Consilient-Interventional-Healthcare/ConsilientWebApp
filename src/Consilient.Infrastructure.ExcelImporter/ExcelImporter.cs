using ClosedXML.Excel;
using Consilient.Infrastructure.ExcelImporter.Constants;
using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.Infrastructure.ExcelImporter.Helpers;
using Consilient.Infrastructure.ExcelImporter.Models;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.RegularExpressions;

namespace Consilient.Infrastructure.ExcelImporter
{
    public class ExcelImporter(ExcelImporterConfiguration configuration, ILoggerFactory loggerFactory) : IExcelImporter
    {
        private readonly ExcelImporterConfiguration _configuration = configuration;
        private readonly ILogger _logger = loggerFactory.CreateLogger<ExcelImporter>();

        public IEnumerable<PatientData> Import(string filename)
        {
            var allPatients = new List<PatientData>();
            _logger.LogInformation("Starting Excel import for file: {FileName}", filename);

            using var workbook = WorkbookFactory.Create(_configuration.CanConvertFile, filename);

            var filteredWorksheets = FilterWorksheets(workbook.Worksheets, _configuration.WorksheetFilters).ToList();
            _logger.LogInformation("Found {WorksheetCount} worksheets to process after filtering.", filteredWorksheets.Count);

            foreach (var worksheet in filteredWorksheets)
            {
                var patientsInSheet = ProcessWorksheet(worksheet);
                allPatients.AddRange(patientsInSheet);
            }

            _logger.LogInformation("Excel import finished for file: {FileName}", filename);
            return allPatients;
        }

        private List<PatientData> ProcessWorksheet(IXLWorksheet worksheet)
        {
            _logger.LogDebug("Processing worksheet: {WorksheetName}", worksheet.Name);

            var (headerRow, columnMap) = FindHeader(worksheet);
            if (headerRow == null || columnMap == null)
            {
                _logger.LogWarning("Header row not found in worksheet '{WorksheetName}'. Skipping.", worksheet.Name);
                return [];
            }

            return ProcessDataRows(worksheet, headerRow, columnMap);
        }

        private (IXLRow? headerRow, Dictionary<string, int>? columnMap) FindHeader(IXLWorksheet worksheet)
        {
            foreach (var row in worksheet.RowsUsed())
            {
                if (IsHeaderRow(row, out var foundHeaders))
                {
                    var columnMap = GetColumnMap(foundHeaders);
                    _logger.LogDebug("Header row found on row {RowNumber} in worksheet '{WorksheetName}'.", row.RowNumber(), worksheet.Name);
                    return (row, columnMap);
                }
            }
            return (null, null);
        }

        private List<PatientData> ProcessDataRows(IXLWorksheet worksheet, IXLRow headerRow, Dictionary<string, int> columnMap)
        {
            var patients = new List<PatientData>();
            var firstRow = headerRow.RowNumber() + 1;
            var lastRowUsed = worksheet.LastRowUsed();
            if (lastRowUsed == null)
            {
                return patients;
            }

            var lastRow = lastRowUsed.RowNumber();
            var dataRows = worksheet.Rows(firstRow, lastRow);

            foreach (var row in dataRows)
            {
                try
                {
                    var patient = new PatientData
                    {
                        CaseID = row.Cell(columnMap[ExcelHeader.CaseId]).GetValue<string>(),
                        Name = row.Cell(columnMap[ExcelHeader.Name]).GetValue<string>(),
                        MRN = row.Cell(columnMap[ExcelHeader.Mrn]).GetValue<string>(),
                        Sex = row.Cell(columnMap[ExcelHeader.Sex]).GetValue<string>(),
                        Age = row.Cell(columnMap[ExcelHeader.Age]).GetValue<int>(),
                        DOB = row.Cell(columnMap[ExcelHeader.Dob]).GetValue<DateTime?>(),
                        Room = row.Cell(columnMap[ExcelHeader.Room]).GetValue<string>(),
                        Bed = row.Cell(columnMap[ExcelHeader.Bed]).GetValue<string>(),
                        DOA = row.Cell(columnMap[ExcelHeader.Doa]).GetValue<DateTime>(),
                        LOS = row.Cell(columnMap[ExcelHeader.Los]).GetValue<int>(),
                        AttendingPhysician = row.Cell(columnMap[ExcelHeader.AttendingPhysician]).GetValue<string>(),
                        PrimaryInsurance = row.Cell(columnMap[ExcelHeader.PrimaryInsurance]).GetValue<string>(),
                        AdmDx = row.Cell(columnMap[ExcelHeader.AdmDx]).GetValue<string>()
                    };
                    patients.Add(patient);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing row {RowNumber} in worksheet '{WorksheetName}'.", row.RowNumber(), worksheet.Name);
                }
            }
            return patients;
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

        internal static IEnumerable<IXLWorksheet> FilterWorksheets(IXLWorksheets worksheets, IEnumerable<string> worksheetFilters)
        {
            if (worksheetFilters == null || !worksheetFilters.Any())
            {
                return worksheets;
            }

            var patterns = worksheetFilters.Select(filter => new Regex(filter, RegexOptions.IgnoreCase)).ToList();

            return worksheets.Where(ws => patterns.Any(p => p.IsMatch(ws.Name)));
        }
    }
}