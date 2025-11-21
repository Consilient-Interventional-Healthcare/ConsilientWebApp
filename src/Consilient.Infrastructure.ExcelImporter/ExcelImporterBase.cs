using ClosedXML.Excel;
using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.Infrastructure.ExcelImporter.Helpers;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Consilient.Infrastructure.ExcelImporter
{


    public abstract class ExcelImporterBase<TData>(
        ExcelImporterConfiguration configuration,
        ILogger logger) : IExcelImporter<TData>
        where TData : class, new()
    {
        protected ILogger Logger { get; } = logger;

        public IEnumerable<TData> Import(string filename)
        {
            Logger.LogInformation("Starting Excel import for file: {FileName}", filename);

            using var workbook = WorkbookFactory.Create(configuration.CanConvertFile, filename);

            var filteredWorksheets =
                FilterWorksheets(workbook.Worksheets, [.. configuration.WorksheetFilters]).ToList();
            Logger.LogInformation("Found {WorksheetCount} worksheets to process after filtering.",
                filteredWorksheets.Count);

            var allPatients = filteredWorksheets.SelectMany(ProcessWorksheet).ToList();

            //foreach (var worksheet in filteredWorksheets)
            //{
            //    var patientsInSheet = ProcessWorksheet(worksheet);
            //    allPatients.AddRange(patientsInSheet);
            //}

            Logger.LogInformation("Excel import finished for file: {FileName}", filename);
            return allPatients;
        }

        internal static IEnumerable<IXLWorksheet> FilterWorksheets(IXLWorksheets worksheets,
            IList<string> worksheetFilters)
        {
            if (!worksheetFilters.Any())
            {
                return worksheets;
            }

            var patterns = worksheetFilters.Select(filter => new Regex(filter, RegexOptions.IgnoreCase)).ToList();

            return worksheets.Where(ws => patterns.Any(p => p.IsMatch(ws.Name)));
        }

        protected abstract TData ExtractEntity(IXLRow row, IDictionary<string, int> columnMap);

        protected abstract (IXLRow? headerRow, Dictionary<string, int>? columnMap) FindHeader(IXLWorksheet worksheet);

        private List<TData> ProcessDataRows(IXLWorksheet worksheet, IXLRow headerRow, Dictionary<string, int> columnMap)
        {
            var rows = new List<TData>();
            var firstRow = headerRow.RowNumber() + 1;
            var lastRowUsed = worksheet.LastRowUsed();
            if (lastRowUsed == null)
            {
                return rows;
            }

            var lastRow = lastRowUsed.RowNumber();
            var dataRows = worksheet.Rows(firstRow, lastRow);

            foreach (var row in dataRows)
            {
                try
                {
                    var entity = ExtractEntity(row, columnMap);
                    rows.Add(entity);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error processing row {RowNumber} in worksheet '{WorksheetName}'.",
                        row.RowNumber(), worksheet.Name);
                }
            }

            return rows;
        }

        private List<TData> ProcessWorksheet(IXLWorksheet worksheet)
        {
            Logger.LogDebug("Processing worksheet: {WorksheetName}", worksheet.Name);

            var (headerRow, columnMap) = FindHeader(worksheet);
            if (headerRow != null && columnMap != null)
            {
                return ProcessDataRows(worksheet, headerRow, columnMap);
            }
            Logger.LogWarning("Header row not found in worksheet '{WorksheetName}'. Skipping.", worksheet.Name);
            return [];
        }
    }
}